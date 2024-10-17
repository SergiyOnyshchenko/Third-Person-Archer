using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using DG.Tweening;
using UnityEngine;

public class TryAutoSelectAnotherWeaponState : ProcessState, IActorIniter
{
    private WeaponInventory _weaponEquipper;
    private Health _health;
    private ShootingTargets _shootingTaragets;


    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out WeaponInventory inventory))
        {
            _weaponEquipper = inventory;
        }

        if (actor.TryGetSystem(out Health health))
        {
            _health = health;
        }

        if (actor.TryGetProperty(out ShootingTargets shootingTargets))
        {
            _shootingTaragets = shootingTargets;
        }
    }

    public override void Enter()
    {
        base.Enter();

        if (_weaponEquipper.TryEquipNext())
        {
            FinishProcess();
        }
        else
        {
            if (_shootingTaragets.Count > 1)
            {
                _health.Die();
            }
            else
            {
                DOVirtual.DelayedCall(3f, () =>
                {
                    if (_shootingTaragets.Count >= 1)
                        _health.Die();
                });
            }
        }
    }
}
