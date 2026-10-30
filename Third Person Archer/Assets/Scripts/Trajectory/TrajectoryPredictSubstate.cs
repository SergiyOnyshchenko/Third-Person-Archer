using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class TrajectoryPredictSubstate : SubState, IActorIniter
{
    [SerializeField] private PlayerProjectileShooter _shooter;
    [SerializeField] private PlayerTrajectoryController.PreviewOriginMode _previewOriginMode;
    [Tooltip("Draw at which the weapon can fire (bow: 0.5). Below it the arc is shown as 'not ready yet'.")]
    [SerializeField, Range(0f, 1f)] private float _readyPull;
    [Tooltip("Launch strength used for draws weaker than this (a quick tap still throws). The shot speed follows the draw above it.")]
    [SerializeField, Range(0f, 1f)] private float _minLaunchPull;
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
        int projectileLayer = _shooter.Prefab != null ? _shooter.Prefab.gameObject.layer : -1;
        _trajectoryController.InitPredictor(_shooter.Profile, _shooter.ShootPoint, _readyPull, _minLaunchPull, projectileLayer);
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