using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Actor;

public class HideMoveState : ProcessState, IActorIniter
{
    [SerializeField] private float _searchRadius = 15f;
    [SerializeField] private LayerMask _hidePointLayer;
    [SerializeField] private PublicStateTransition _skipTransition;

    private MoveInput _moverInput;
    private NavMeshAgent _agent;
    private HidePoint _currentHidePoint;
    private HidePoint _lastHidePoint;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput mover))
            _moverInput = mover;
        _agent = actor.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();
        StartCoroutine(MoveToHide());
    }

    public override void Exit()
    {
        _lastHidePoint = _currentHidePoint;
        _currentHidePoint?.Release();
        _currentHidePoint = null;
        base.Exit();
    }

    private IEnumerator MoveToHide()
    {
        yield return null;

        HidePoint target = FindBestHidePoint();

        if (target == null)
        {
            _skipTransition.Transit();
            yield break;
        }

        _currentHidePoint = target;
        _currentHidePoint.Occupy();
        _moverInput.MoveToDestination(_currentHidePoint.transform.position);
    }

    private HidePoint FindBestHidePoint()
    {
        Collider[] hits = Physics.OverlapSphere(_agent.transform.position, _searchRadius, _hidePointLayer);

        var candidates = new List<HidePoint>();

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out HidePoint point)) continue;
            if (point.IsOccupied) continue;
            if (!IsPathValid(point.transform.position)) continue;
            candidates.Add(point);
        }

        if (candidates.Count == 0) return null;

        List<HidePoint> preferred = candidates.Count > 1
            ? candidates.FindAll(p => p != _lastHidePoint)
            : candidates;

        if (preferred.Count == 0) preferred = candidates;

        HidePoint best = null;
        float bestDist = float.MaxValue;
        Vector3 agentPos = _agent.transform.position;

        foreach (var point in preferred)
        {
            float dist = Vector3.Distance(agentPos, point.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = point;
            }
        }

        return best;
    }

    private bool IsPathValid(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        return _agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
    }
}
