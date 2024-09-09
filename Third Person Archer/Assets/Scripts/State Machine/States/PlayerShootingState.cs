using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using DG.Tweening;

public class PlayerShootingState : ProcessState, IActorIniter
{
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private ActorController[] _hostages;
    [SerializeField] private Transform _lookAtPoint;
    [SerializeField] private float _delay = 0.1f;
    [SerializeField] private bool _triggerEnemiesOnEnter;
    [SerializeField] private bool _hideEnemiesBeforeShooting = true;
    private AttackInput _attackInput;
    private ActorController _player;
    private ShootingTargets _shootingTargets;

    protected override void Awake()
    {
        base.Awake();

        if (_hideEnemiesBeforeShooting)
        {
            HideEnemiesBeforeSgooting();
        }
    }

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

        DOVirtual.DelayedCall(_delay, () =>
        {
            ActivateEnemies();
            _attackInput.AllowAttack(true);

            if (_lookAtPoint != null && _player.TryGetSystem(out BodyRotator rotator))
                rotator.RotateToInstant(_lookAtPoint);
        });

        ITarget playerTarget = null;

        if (_player.TryGetSystem(out Actor.Target target))
            playerTarget = target;

        if (_triggerEnemiesOnEnter)
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                PerceptionInput input = _enemies[i].GetComponentInChildren<PerceptionInput>();
                input.ActivatePerception(new ITarget[] { playerTarget });
                input.ReciveSound("", 1f, _player.gameObject);
            }
        }
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
            data.InitShootingTargets(_enemies, _hostages);
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

        foreach (var hostage in _hostages)
            if (hostage.TryGetInput(out PerceptionInput perception))
                perception.ActivatePerception(targetsForEnemies);

        if (_shootingTargets != null)
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

    private void HideEnemiesBeforeSgooting()
    {
        int index = transform.GetSiblingIndex();

        if (index != 0)
        {
            Transform previousState = transform.parent.GetChild(index - 1);

            if (previousState.TryGetComponent(out MainState state))
            {
                ShowStateNextEnemies showStateNextEnemiesstate = gameObject.AddComponent<ShowStateNextEnemies>();
                showStateNextEnemiesstate.Init(_enemies, state, true);
            }
        }

        if (index != transform.parent.childCount - 1)
        {
            Transform nextState = transform.parent.GetChild(index + 1);

            if (nextState.TryGetComponent(out MainState state))
            {
                ShowStateNextEnemies showStateNextEnemiesstate = gameObject.AddComponent<ShowStateNextEnemies>();
                showStateNextEnemiesstate.Init(_enemies, state, false);
            }
        }
    }
}
