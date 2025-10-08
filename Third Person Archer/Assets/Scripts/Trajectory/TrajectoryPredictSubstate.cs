using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class TrajectoryPredictSubstate : SubState, IActorIniter
{
    [SerializeField] private PlayerProjectileShooter _shooter;
    [SerializeField] private PlayerTrajectoryController.PreviewOriginMode _previewOriginMode;
    private PlayerTrajectoryController _trajectoryController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _trajectoryController)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _trajectoryController.SetOriginMode(_previewOriginMode);
        _trajectoryController.InitPredictor(_shooter.Profile, _shooter.ShootPoint);
    }

    private void Update()
    {
        _trajectoryController.UpdateTrajectory();
    }

    public override void Exit()
    {
        _trajectoryController.Reset();

        base.Exit();
    }
}