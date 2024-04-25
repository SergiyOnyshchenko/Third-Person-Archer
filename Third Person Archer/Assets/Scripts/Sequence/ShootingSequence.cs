using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ShootingSequence : Sequence, ITargetActor
{
    [SerializeField] private ActorController[] _enemies;
    public ActorController TargetActor { get; set; }


    private void StartShooting()
    {
        if (TargetActor.TryGetInput(out ShootingInput shootInput))
            shootInput.StartShooting();
    }

    private void FinishShooting()
    {
        if (TargetActor.TryGetInput(out ShootingInput shootInput))
            shootInput.FinishShooting();
    }
}
