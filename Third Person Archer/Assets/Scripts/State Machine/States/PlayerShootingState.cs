using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using DG.Tweening;

public class PlayerShootingState : ProcessState, IActorIniter
{
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private Transform _lookAtPoint;
    private AttackInput _attackInput;
    private ActorController _player;
    private ShootingTargets _shootingTargets;

    public void InitActor(ActorController actor)
    {
        _player = actor;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetProperty(out ShootingTargets shootingTargets))
            _shootingTargets = shootingTargets;
    }

    public override void Enter()
    {
        base.Enter();

        InitShootingData();

        DOVirtual.DelayedCall(0.1f, () =>
        {
            ActivateEnemies();
            _attackInput.AllowAttack(true);

            if (_lookAtPoint != null && _player.TryGetSystem(out BodyRotator rotator))
                rotator.RotateToInstant(_lookAtPoint);
        });
    }

    public override void Exit()
    {
        _attackInput.AllowAttack(false);
        base.Exit();
    }

    private void InitShootingData()
    {
        IShootingTargetsData[] shootingData = GetComponentsInChildren<IShootingTargetsData>();

        foreach (var data in shootingData)
            data.InitShootingTargets(_enemies);
    }

    private void ActivateEnemies()
    {
        ITarget playerTarget;

        if (_player.TryGetSystem(out Actor.Target target))
            playerTarget = target;
        else
            return;

        ITarget[] targetsForEnemies = new ITarget[] { playerTarget };

        foreach (var enemy in _enemies)
            if (enemy.TryGetInput(out PerceptionInput perception))
                perception.ActivatePerception(targetsForEnemies);

        if(_shootingTargets != null)
        {
            List<ITarget> targets = new List<ITarget>();

            foreach (var enemy in _enemies)
            {
                if (enemy.TryGetSystem(out Actor.Target targetEnemy))
                    targets.Add(targetEnemy);
            }

            _shootingTargets.Init(targets);
        }
    }

    private void TryGetEnemies()
    {
        var childEnemies = GetComponentsInChildren<ActorController>();

        List<ActorController> allEnemies = new List<ActorController>(_enemies);

        foreach (var childEneemy in childEnemies)
        {
            bool hasEnemy = false;

            foreach (var enemy in _enemies)
            {
                if (childEneemy == enemy)
                {
                    hasEnemy = true;
                    break;
                }
            }

            if (!hasEnemy)
                allEnemies.Add(childEneemy);
        }

        _enemies = allEnemies.ToArray();
    }
}
