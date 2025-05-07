using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class ReviveState : MainState, IActorIniter
{
    private ActorController _actor;
    private RagdollControll _ragdoll;
    private Health _health;
    private BowAmmo _bowAmmo;
    private CrossbowAmmo _crossbowAmmo;
    private SpearAmmo _spearAmmo;


    public void InitActor(ActorController actor)
    {
        _actor = actor;
        _ragdoll = actor.GetComponentInChildren<RagdollControll>();

        if (actor.TryGetSystem(out Health health))
            _health = health;

        if (actor.TryGetProperty(out _bowAmmo)) { }
        if (actor.TryGetProperty(out _crossbowAmmo)) { }
        if (actor.TryGetProperty(out _spearAmmo)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _actor.ReviveHandler();
        _health.Revive();

        _bowAmmo.ResetAmmoCount();
        _crossbowAmmo.ResetAmmoCount();
        _spearAmmo.ResetAmmoCount();

        if (_ragdoll != null)
            _ragdoll.MakeKinematic();
    }
}
