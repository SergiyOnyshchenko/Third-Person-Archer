using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.AI;

public class NpcRunawayState : MainState, IActorIniter
{
    public static class SearchBudget
    {
        // How many candidate evaluations across the whole game per frame.
        // Tune this once; e.g., 300–1200 depending on platform/scene.
        public static int GlobalMaxAttemptsPerFrame = 300;

        private static int _frameCount = -1;
        private static int _usedThisFrame = 0;

        /// <summary>Call once per attempt; returns true if budget is available and consumes one unit.</summary>
        public static bool TryConsume()
        {
            int current = Time.frameCount;
            if (current != _frameCount)
            {
                _frameCount = current;
                _usedThisFrame = 0;
            }

            if (_usedThisFrame >= GlobalMaxAttemptsPerFrame)
                return false;

            _usedThisFrame++;
            return true;
        }

    }

    [Header("Search Area")]
    [SerializeField] private float _minDistance = 6f;
    [SerializeField] private float _maxDistance = 20f;
    [SerializeField] private float _navMeshSampleMaxDist = 2f;
    [Tooltip("Total attempts for one search (spread over frames).")]
    [SerializeField] private int _maxCandidatesPerSearch = 60;

    [Header("Per-Frame Throttling")]
    [Tooltip("Per-NPC cap so a single NPC doesn't consume the whole global budget in one frame.")]
    [SerializeField] private int _maxAttemptsPerFramePerNpc = 8;

    [Header("Visibility / Occlusion")]
    [Tooltip("Layers considered as occluders from camera to point.")]
    [SerializeField] private LayerMask _occluderLayers = ~0;
    [Tooltip("Layers used by the LOS raycast (usually matches occluders; exclude NPC layer).")]
    [SerializeField] private LayerMask _losRaycastMask = ~0;
    [Tooltip("Height offset (eye height) for visibility checks.")]
    [SerializeField] private float _visibilityHeightOffset = 1.6f;

    [Header("Validation / Recheck")]
    [Tooltip("Randomized recheck interval (seconds) to validate/refresh target.")]
    [SerializeField] private Vector2 _recheckIntervalSeconds = new Vector2(2f, 5f);
    [Tooltip("If true, target becomes invalid when visible (and not occluded).")]
    [SerializeField] private bool _invalidateIfVisible = true;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color _targetColor = new Color(0.25f, 1f, 0.25f, 0.9f);
    private Camera _camera;                 // Defaults to Camera.main if null
    private NavMeshAgent _agent;
    private Vector3 _currentTarget;
    private bool _hasTarget;

    private Coroutine _mainLoop;
    private Coroutine _activeSearch;

    private Mover _mover;

    public void InitActor(ActorController actor)
    {
        _camera = Camera.main;
        _agent = actor.GetComponent<NavMeshAgent>();

        if (actor.TryGetSystem(out _mover)){}
    }

    public override void Enter()
    {
        base.Enter();

        if (_mainLoop == null)
            _mainLoop = StartCoroutine(MainLoop());
    }

    public override void Exit()
    {
        StopAllRunningCoroutines();

        base.Exit();
    }

    private void StopAllRunningCoroutines()
    {
        if (_activeSearch != null)
        {
            StopCoroutine(_activeSearch);
            _activeSearch = null;
        }

        if (_mainLoop != null)
        {
            StopCoroutine(_mainLoop);
            _mainLoop = null;
        }
    }


    #region Main Loop

    private IEnumerator MainLoop()
    {
        // Initial search
        yield return StartSearchAndWait();

        while (true)
        {
            float wait = UnityEngine.Random.Range(_recheckIntervalSeconds.x, _recheckIntervalSeconds.y);
            yield return new WaitForSeconds(wait);

            if (!_hasTarget || !IsPointStillValid(_currentTarget))
            {
                yield return StartSearchAndWait();
            }
        }
    }

    public void ForceRecompute()
    {
        if (!isActiveAndEnabled) return;

        if (_activeSearch != null) StopCoroutine(_activeSearch);
        _activeSearch = StartCoroutine(TryFindRunawayPointAsync(OnFoundTarget));
    }

    private IEnumerator StartSearchAndWait()
    {
        if (_activeSearch != null) StopCoroutine(_activeSearch);

        bool done = false;
        _activeSearch = StartCoroutine(TryFindRunawayPointAsync(pos =>
        {
            _currentTarget = pos;
            _hasTarget = true;
            MoveTo(_currentTarget);
            done = true;
        }));

        while (!done && _activeSearch != null)
            yield return null;
    }

    private void OnFoundTarget(Vector3 pos)
    {
        _currentTarget = pos;
        _hasTarget = true;
        MoveTo(_currentTarget);
    }

    #endregion
    
        #region Search Logic (Budgeted, Visibility-first)

    /// <summary>
    /// Finds a valid runaway point using per-frame throttling and global budget.
    /// Visibility checks are done before NavMesh pathing to avoid expensive calls when unnecessary.
    /// </summary>
    private IEnumerator TryFindRunawayPointAsync(Action<Vector3> onSuccess)
    {
        int attempts = 0;
        int attemptsThisFrameForThisNpc = 0;
        int frameStamp = Time.frameCount;

        while (attempts < _maxCandidatesPerSearch)
        {
            // Reset per-frame per-NPC counter when the frame advances
            if (Time.frameCount != frameStamp)
            {
                frameStamp = Time.frameCount;
                attemptsThisFrameForThisNpc = 0;
            }

            // Budget check: global + per-NPC cap
            if (attemptsThisFrameForThisNpc >= _maxAttemptsPerFramePerNpc || !SearchBudget.TryConsume())
            {
                attemptsThisFrameForThisNpc = 0;
                yield return null; // push to next frame
                continue;
            }

            attempts++;
            attemptsThisFrameForThisNpc++;

            // 1) Generate candidate (cheap)
            Vector3 candidateRaw = SampleRandomXZ(transform.position, _minDistance, _maxDistance);

            // 2) Visibility (cheap) — use VisibilityUtility
            Vector3 losPoint = candidateRaw + Vector3.up * _visibilityHeightOffset;
            bool inFrustum = VisibilityUtility.IsPointInCameraFrustum(_camera, losPoint);
            bool occluded = VisibilityUtility.IsOccludedFromCamera(_camera, losPoint, _losRaycastMask, _occluderLayers);

            // We accept candidates that are either outside frustum OR occluded.
            if (!inFrustum || occluded)
            {
                // 3) Snap to NavMesh (moderate)
                if (NavMesh.SamplePosition(candidateRaw, out NavMeshHit hit, _navMeshSampleMaxDist, NavMesh.AllAreas))
                {
                    // 4) Full path (expensive)
                    if (IsReachable(hit.position))
                    {
                        onSuccess?.Invoke(hit.position);
                        _activeSearch = null;
                        yield break;
                    }
                }
            }

            // If we reached our per-NPC cap this frame, yield
            if (attemptsThisFrameForThisNpc >= _maxAttemptsPerFramePerNpc)
            {
                attemptsThisFrameForThisNpc = 0;
                yield return null;
            }
        }

        // No valid point found this round
        _activeSearch = null;
    }

    private bool IsPointStillValid(Vector3 point)
    {
        if (!IsReachable(point)) return false;

        if (_invalidateIfVisible)
        {
            Vector3 p = point + Vector3.up * _visibilityHeightOffset;
            bool inFrustum = VisibilityUtility.IsPointInCameraFrustum(_camera, p);
            bool occluded = VisibilityUtility.IsOccludedFromCamera(_camera, p, _losRaycastMask, _occluderLayers);
            if (inFrustum && !occluded) return false;
        }

        return true;
    }

    #endregion

    #region Helpers

    private static Vector3 SampleRandomXZ(Vector3 center, float minRadius, float maxRadius)
    {
        Vector2 rnd = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minRadius, maxRadius);
        // slight upward nudge to avoid ground clipping issues during SamplePosition
        return new Vector3(center.x + rnd.x, center.y + 1.0f, center.z + rnd.y);
    }

    private bool IsReachable(Vector3 destination)
    {
        if (_agent == null || !_agent.isOnNavMesh) return false;

        var path = new NavMeshPath();
        if (!_agent.CalculatePath(destination, path)) return false;

        return path.status == NavMeshPathStatus.PathComplete;
        // If you prefer "partial okay", change to: path.status != NavMeshPathStatus.PathInvalid
    }

    /// <summary>
    /// Your movement hook. Replace with your mover / uncomment agent call.
    /// </summary>
    private void MoveTo(Vector3 destination)
    {
        _mover.Move(destination, null);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _minDistance);
        Gizmos.DrawWireSphere(transform.position, _maxDistance);

        if (_hasTarget)
        {
            Gizmos.color = _targetColor;
            Gizmos.DrawSphere(_currentTarget + Vector3.up * 0.1f, 0.25f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, _currentTarget + Vector3.up * 0.1f);
        }
    }

    #endregion
}