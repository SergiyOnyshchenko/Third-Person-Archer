using System.Collections;
using System.Collections.Generic;
using System.IO;
using Actor;
using Actor.Properties;
using UnityEngine;

public class PatrolState : MainState, IActorIniter
{
    private MoveInput _moverInput;
    private Mover _mover;
    private PatrolPath _path;
    private int _movePointIndex;
    private IEnumerator _moveNextPoint;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput moveInput))
            _moverInput = moveInput;

        if (actor.TryGetSystem(out Mover mover))
            _mover = mover;

        if (actor.TryGetProperty(out PatrolPath path))
            _path = path;
    }

    public override void Enter()
    {
        base.Enter();

        _movePointIndex++;
        Move();
    }

    public override void Exit()
    {
        _mover.Stop();

        if (_moveNextPoint != null)
            StopCoroutine(_moveNextPoint);

        _mover.OnMovingFinished.RemoveListener(MoveNextPathPoint);

        base.Exit();
    }

    private void Move()
    {
        Vector3[] path = _path.GetPath();

        if(_movePointIndex >= path.Length)
            _movePointIndex = 0;

        _moverInput.MoveToDestination(path[_movePointIndex]);
        _mover.OnMovingFinished.AddListener(MoveNextPathPoint);
    }

    private void MoveNextPathPoint()
    {
        _mover.OnMovingFinished.RemoveListener(MoveNextPathPoint);

        if(_moveNextPoint != null)
            StopCoroutine(_moveNextPoint);

        _moveNextPoint = MoveNextPointWithDelay(1);
        StartCoroutine(_moveNextPoint);
    }

    private IEnumerator MoveNextPointWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _movePointIndex++;
        Move();
    }
}
