using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class PlayerShootingState : ProcessState, IActorIniter
{
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private Transform _lookAtPoint;
    private AttackInput _attackInput;
    private ActorController _player;

    public void InitActor(ActorController actor)
    {
        _player = actor;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
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

        if (_player.TryGetSystem(out Target target))
            playerTarget = target;
        else
            return;

        ITarget[] targetsForEnemies = new ITarget[] { playerTarget };

        foreach (var enemy in _enemies)
            if (enemy.TryGetInput(out PerceptionInput perception))
                perception.ActivatePerception(targetsForEnemies);
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
