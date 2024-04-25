using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class PlayerShootingState : ProcessState, IActorIniter
{
    [SerializeField] private ActorController[] _enemies;
    private ShootingInput _shootingInput;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out ShootingInput shootInput))
            _shootingInput = shootInput;
    }

    public override void Enter()
    {
        base.Enter();
        _shootingInput.StartShooting();
    }

    public override void Exit()
    {
        _shootingInput.FinishShooting();
        base.Exit();
    }
}
