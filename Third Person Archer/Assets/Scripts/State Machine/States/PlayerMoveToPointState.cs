using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveToPointState : ProcessState, IActorIniter
{
    [Header("Destination")]    
    [SerializeField] private Transform _destination;
    [Header("NavMesh Snap Settings")]
    [SerializeField, Min(0.01f)] private float _snapMaxDistance = 2.0f;
    [SerializeField] private int _areaMask = NavMesh.AllAreas;
    private Transform _target;
    private MoveInput _mover;

    public void InitActor(ActorController actor)
    {
        _target = actor.transform;

        if (actor.TryGetInput(out MoveInput mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        if (_mover == null)
        {
            Debug.LogError($"[{nameof(PlayerMoveToPointState)}] MoveInput not found.");
            return;
        }

        if (_destination == null)
        {
            Debug.LogError($"[{nameof(PlayerMoveToPointState)}] Destination is not assigned.");
            return;
        }

        // Snap destination transform onto the NavMesh (or closest point within radius)
        Vector3 desired = _destination.position;
        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, _snapMaxDistance, _areaMask))
        {
            // Move the marker onto the exact navmesh point so downstream code stays unchanged
            _destination.position = hit.position;
        }
        else
        {
            Debug.LogWarning(
                $"[{nameof(PlayerMoveToPointState)}] Couldn't find NavMesh near destination (radius={_snapMaxDistance}). Using original position.");
        }

        // Keep your small delay if needed for initialization ordering
        DOVirtual.DelayedCall(0.1f, () => _mover.MoveToDestination(_destination));
    }
}