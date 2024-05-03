using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.ThirdPerson;
using WeaponController = Actor.ThirdPerson.WeaponController;

public class ShootingState : MainState, IActorIniter
{
    private WeaponController _weaponController;
    private PerceptionInput _input;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out WeaponController weaponController))
            _weaponController = weaponController;

        if (actor.TryGetInput(out PerceptionInput input))
            _input = input;
    }

    public override void Enter()
    {
        base.Enter();

        _weaponController.SetFire();
        _weaponController.SetTarget(_input.Target.transform);
    }
}
