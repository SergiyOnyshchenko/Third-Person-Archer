using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class TrajectoryPredictSubstate : SubState, IActorIniter
{
    [SerializeField] private PlayerProjectileShooter _shooter;
    [SerializeField] private PlayerTrajectoryController.PreviewOriginMode _previewOriginMode;
    private PlayerTrajectoryController _trajectoryController;
    private ShootTypeOverride _shootTypeOverride;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _trajectoryController)) { }
        if (actor.TryGetProperty(out _shootTypeOverride)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_shootTypeOverride != null && _shootTypeOverride.Value == ShootType.Direct)
            return;

        _trajectoryController.SetOriginMode(_previewOriginMode);
        _trajectoryController.InitPredictor(_shooter.Profile, _shooter.ShootPoint);
    }

    private void Update()
    {
        if (_shootTypeOverride != null && _shootTypeOverride.Value == ShootType.Direct)
            return;

        _trajectoryController.UpdateTrajectory();
    }

    public override void Exit()
    {
        if (_shootTypeOverride != null && _shootTypeOverride.Value == ShootType.Direct)
            return;

        _trajectoryController.Reset();

        base.Exit();
    }
}