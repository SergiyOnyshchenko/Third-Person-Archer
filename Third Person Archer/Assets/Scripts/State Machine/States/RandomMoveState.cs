using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;
using UnityEngine.AI;

public class RandomMoveState : ProcessState, IActorIniter
{
    [SerializeField] private float _rangeMin = 3f;
    [SerializeField] private float _rangeMax = 7.5f;
    private NavMeshAgent _agent;
    private MoveInput _moverInput;
    private SpawnPoint _spawnPoint;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput mover))
            _moverInput = mover;

        if(actor.TryGetProperty(out SpawnPoint spawnPoint))
            _spawnPoint = spawnPoint;

        _agent = actor.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();
        StartCoroutine(MoveRandom());
    }

    public IEnumerator MoveRandom()
    {
        Vector3 movePosition;

        while (!TryCalculatePath(out movePosition))
        {
            yield return null;
        }
            
        _moverInput.MoveToDestination(movePosition);
    }

    private bool TryCalculatePath(out Vector3 movePosition)
    {
        movePosition = new Vector3(
            Random.Range(-_rangeMax, _rangeMax),
            0,
            Random.Range(-_rangeMax, _rangeMax));

        movePosition += _spawnPoint.Value;

        if(Vector3.Distance(_agent.transform.position, movePosition) < _rangeMin)
            return false;

        NavMeshPath navMeshPath = new NavMeshPath();

        if (_agent.CalculatePath(movePosition, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            return true;
        else
            return false;
    }
}
