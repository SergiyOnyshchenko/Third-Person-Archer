using Actor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class PlayerWallRunState : ProcessState, IActorIniter
{
    [Header("Wall Run Path")]
    [SerializeField] private Transform[] _path;
    [SerializeField] private bool _isRightDirection;

    [Header("NavMesh Snap")]
    [SerializeField, Min(0.01f)] private float _snapMaxDistance = 2.0f;
    [SerializeField] private int _areaMask = NavMesh.AllAreas;

    private WallRunInput _wallRun;
    private MoveInput _mover;
    private NavMeshAgent _agent;

    private GameObject _entryPointGO;
    private GameObject _exitPointGO;
    private Tween _arrivalCheckTween;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out _wallRun)) { }
        if (actor.TryGetInput(out _mover)) { }
        _agent = actor.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();

        if (_path == null || _path.Length < 2)
        {
            Debug.LogError($"[{nameof(PlayerWallRunState)}] Path must have at least 2 points.");
            return;
        }

        _entryPointGO = CreateNavMeshPoint("EntryPoint", _path[0].position);
        _exitPointGO  = CreateNavMeshPoint("ExitPoint",  _path[_path.Length - 1].position);

        if (_entryPointGO == null || _exitPointGO == null)
        {
            Debug.LogError($"[{nameof(PlayerWallRunState)}] Failed to snap entry/exit points to NavMesh.");
            return;
        }

        DOVirtual.DelayedCall(0.02f, () =>
        {
            _mover.MoveToDestination(_entryPointGO.transform);
            WaitForArrival(OnArrivedAtEntry);
        });
    }

    public override void Exit()
    {
        base.Exit();
        _arrivalCheckTween?.Kill();
        CleanupPoints();
    }

    // ──────────────────────────────────────────

    private void OnArrivedAtEntry()
    {
        // WallRunInput.StartWallRun должен принимать Action onComplete
        _wallRun.StartWallRun(_path, _isRightDirection, OnWallRunComplete);
    }

    private void OnWallRunComplete()
    {
        if (_agent != null && _exitPointGO != null)
            _agent.Warp(_exitPointGO.transform.position);

        CleanupPoints();
        FinishProcess();

        DOVirtual.DelayedCall(0.02f, () =>
        {
            
        });
    }

    // ──────────────────────────────────────────

    /// <summary>Рекурсивный polling каждые 0.1 с до достижения цели.</summary>
    private void WaitForArrival(System.Action onArrived)
    {
        _arrivalCheckTween?.Kill();

        void Check()
        {
            if (HasReachedDestination())
            {
                onArrived?.Invoke();
                return;
            }
            _arrivalCheckTween = DOVirtual.DelayedCall(0.1f, Check);
        }

        _arrivalCheckTween = DOVirtual.DelayedCall(0.1f, Check);
    }

    private bool HasReachedDestination()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || _agent.pathPending)
            return false;

        return _agent.remainingDistance <= _agent.stoppingDistance + 0.05f;
    }

    // ──────────────────────────────────────────

    private GameObject CreateNavMeshPoint(string pointName, Vector3 nearPosition)
    {
        if (!NavMesh.SamplePosition(nearPosition, out NavMeshHit hit, _snapMaxDistance, _areaMask))
        {
            Debug.LogWarning(
                $"[{nameof(PlayerWallRunState)}] Couldn't snap '{pointName}' to NavMesh " +
                $"near {nearPosition} (radius={_snapMaxDistance}).");
            return null;
        }

        var go = new GameObject($"[WallRun] {pointName}");
        go.transform.position = hit.position;
        return go;
    }

    private void CleanupPoints()
    {
        if (_entryPointGO != null) Object.Destroy(_entryPointGO);
        if (_exitPointGO  != null) Object.Destroy(_exitPointGO);
        _entryPointGO = null;
        _exitPointGO  = null;
    }
}