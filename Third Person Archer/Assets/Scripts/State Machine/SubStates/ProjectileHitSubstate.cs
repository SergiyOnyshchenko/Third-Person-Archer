using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class ProjectileHitSubstate : SubState, IActorIniter
{
    private ProjectileLastHitData _hitData;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Collider _collider;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        //_rigidbody = actor.GetComponent<Rigidbody>();
        //_collider = actor.GetComponent<Collider>();

        if (actor.TryGetProperty(out _hitData)) { }
    }

    public override void Enter()
    {
        base.Enter();

        /*
        _collider.enabled = false;
        _rigidbody.isKinematic = true;

        if (_hitData != null && _hitData.Value != null)
        {
            if (_hitData.Value.contacts != null && _hitData.Value.contacts.Length > 0)
                _transform.position = _hitData.Value.contacts[0].point;

            _transform.SetParent(_hitData.Value.transform);
        }
        */
    }
}
