using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ShootingSequence : Sequence, ITargetActor
{
    [SerializeField] private ActorController[] _enemies;
    public ActorController TargetActor { get; set; }

    public override void Begin()
    {
        base.Begin();
    }

    protected override void Finish()
    {
        base.Finish();
    }

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
