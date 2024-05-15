using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTargetState : MainState, IActorIniter
{
    private MoveInput _moverInput;
    private PerceptionInput _perception;
    private IEnumerator _moving;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput mover))
            _moverInput = mover;

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;
    }

    public override void Enter()
    {
        base.Enter();

        if (_moving != null)
            StopCoroutine(_moving);

        _moving = MoveToTarget();

        StartCoroutine(_moving);
    }

    public override void Exit() 
    {
        if (_moving != null)
            StopCoroutine(_moving);

        _moverInput.Stop();

        base.Exit();
    }

    public IEnumerator MoveToTarget()
    {
        while (true)
        {
            ITarget target = _perception.Target;

            if (target != null)
                _moverInput.MoveToDestination(target.TargetPoint);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
