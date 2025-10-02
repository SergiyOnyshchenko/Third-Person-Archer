using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Actor;
using Actor.Properties;

public class NpcPatrolState : MainState, IActorIniter
{
    [Header("NavMesh Projection")]
    [Tooltip("Max distance to search for a nearest NavMesh position for each waypoint.")]
    [SerializeField] private float _projectionMaxDistance = 3f;

    [Tooltip("NavMesh area mask used for projection.")]
    [SerializeField] private int _areaMask = NavMesh.AllAreas;

    [Tooltip("Optional pause at each point (seconds). Set 0 for no wait.")]
    [SerializeField, Min(0f)] private float _waitAtPoint = 0f;

    private Mover _mover;
    private PatrolPath _path;
    private Vector3[] _navPoints; 
    private int _currentIndex = 0;
    private bool _isActive;
    public Vector3[] Points => _path.GetPath();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _mover)) { }
        if (actor.TryGetProperty(out _path)) { }
    }

    public override void Enter()
    {
        base.Enter();

        BuildNavPoints();

        if (!ValidateReady())
            return;

        _isActive = true;
        MoveToCurrent();
    }
    
    public override void Exit()
    {
        _isActive = false;

        if (_mover != null)
        {
            // Stop mover cleanly; consumer may also listen to OnMovingFinished
            _mover.Stop();
        }

        base.Exit();       // fires OutOfState + disables this component
    }

    private bool ValidateReady()
    {
        if (_mover == null)
        {
            Debug.LogError($"No Mover assigned/found.");
            return false;
        }

        if (_navPoints == null || _navPoints.Length == 0)
        {
            Debug.LogWarning($"No valid patrol points.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Build the cached world-space points snapped to the NavMesh.
    /// </summary>
    private void BuildNavPoints()
    {
        if (Points == null || Points.Length == 0)
        {
            _navPoints = System.Array.Empty<Vector3>();
            return;
        }

        var list = new List<Vector3>(Points.Length);

        for (int i = 0; i < Points.Length; i++)
        {
            if (Points[i] == null)
                continue;

            Vector3 source = Points[i];

            // If the transform is not on the NavMesh, snap to the closest valid position.
            if (NavMesh.SamplePosition(source, out NavMeshHit hit, _projectionMaxDistance, _areaMask))
            {
                list.Add(hit.position);
            }
            else
            {
                Debug.LogWarning($"Waypoint {i} not near NavMesh (searched {_projectionMaxDistance}m). Skipping.");
            }
        }

        _navPoints = list.ToArray();
        _currentIndex = 0;
    }

    private void MoveToCurrent()
    {
        if (!_isActive || _navPoints.Length == 0)
            return;

        Vector3 target = _navPoints[_currentIndex];

        // Use the Mover's per-move completion callback to chain movements
        _mover.Move(target, onCompleted: OnReachedPoint);
    }

    private void OnReachedPoint()
    {
        if (!_isActive)
            return;

        // Optionally wait, then proceed
        if (_waitAtPoint > 0f)
        {
            // Use a simple coroutine tied to this state component
            StartCoroutine(WaitAndAdvance(_waitAtPoint));
        }
        else
        {
            AdvanceAndMove();
        }
    }

    private System.Collections.IEnumerator WaitAndAdvance(float seconds)
    {
        float t = 0f;
        while (t < seconds && _isActive)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (_isActive)
            AdvanceAndMove();
    }

    private void AdvanceAndMove()
    {
        _currentIndex = (_currentIndex + 1) % _navPoints.Length;
        MoveToCurrent();
    }
}
