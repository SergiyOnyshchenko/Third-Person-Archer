using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class TargetInRangeTransition : StateTransition, IActorIniter
{
    [SerializeField] private float _range = 1f;
    private PerceptionInput _perception;
    private Transform _transform;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;
    }

    private void Update()
    {
        Debug.Log(1);

        Debug.Log(2);

        if (_perception.Target == null)
            return;

        float distance = Vector3.Distance(_perception.Target.RootPoint.position, _transform.position);

        Debug.Log(distance);

        if (distance > _range)
            return;

        DoTransition();
    }
}
