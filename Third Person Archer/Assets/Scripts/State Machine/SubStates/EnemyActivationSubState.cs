using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public class EnemyActivationSubState : SubState, IActorIniter
{
    [SerializeField] private ActorController[] _enemies;
    private ActorController _player;

    public void InitActor(ActorController actor)
    {
        _player = actor;
    }

    public override void Enter()
    {
        base.Enter();

        ITarget playerTarget;

        if (_player.TryGetSystem(out Actor.Target target))
            playerTarget = target;
        else
            return;

        ITarget[] targetsForEnemies = new ITarget[] { playerTarget };

        foreach (var enemy in _enemies)
            if (enemy.TryGetInput(out PerceptionInput perception))
                perception.ActivatePerception(targetsForEnemies);
    }
}
