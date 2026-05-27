using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class PlayerShootingState : ProcessState, IActorIniter
{
    [Header("Actors")]
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private ActorController[] _hostages;
    [SerializeField] private GameObject[] _additionalObjects;
    [Header("Settings")]
    [SerializeField] private Transform _lookAtPoint;
    [SerializeField] private float _delay = 0.1f;
    [SerializeField] private bool _triggerEnemiesOnEnter;
    [SerializeField] private bool _hideEnemiesBeforeShooting = true;
    [SerializeField] private ActorController _player;
    private AttackInput _attackInput;
    private Health _health;
    private ShootingTargets _shootingTargets;
    private BodyRotator _rotator;
    private NavMeshAgent _agent;
    private Tween _enterDelayTween;
    private bool _previousAgentUpdateRotation;

    protected override void Awake()
    {
        base.Awake();

        if (_hideEnemiesBeforeShooting)
        {
            HideEnemiesBeforeShooting();
        }
    }

    public void InitActor(ActorController actor)
    {
        _player = actor;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetProperty(out ShootingTargets shootingTargets))
            _shootingTargets = shootingTargets;

        if (actor.TryGetSystem(out Health health))
            _health = health;

        if (actor.TryGetSystem(out BodyRotator rotator))
            _rotator = rotator;

        _agent = actor.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();

        InitShootingData();
        PrepareManualRotation();
        RotateToLookAtPoint();

        _enterDelayTween?.Kill();

        _enterDelayTween = DOVirtual.DelayedCall(_delay, () =>
        {
            if (!enabled)
                return;

            ActivateEnemies();
            _attackInput?.AllowAttack(true);

            RotateToLookAtPoint();

            ITarget playerTarget = null;

            if (_player.TryGetSystem(out Actor.Target target))
                playerTarget = target;

            if (_triggerEnemiesOnEnter)
            {
                for (int i = 0; i < _enemies.Length; i++)
                {
                    PerceptionInput input = _enemies[i].GetComponentInChildren<PerceptionInput>();
                    if (input == null)
                        continue;

                    input.ActivatePerception(new ITarget[] { playerTarget });
                    input.ReciveSound("", 1f, _player.gameObject);
                }
            }
        }).SetTarget(this);
    }

    public override void Exit()
    {
        _enterDelayTween?.Kill();
        _enterDelayTween = null;

        RestoreAgentRotation();

        if (!IsNextStatePlayerShootingState())
        {
            if (_rotator != null)
                _rotator.ResetYRotation();

            _attackInput?.AllowAttack(false);
        }

        base.Exit();
    }

    public MainState GetPreviousState()
    {
        int currentIndex = transform.GetSiblingIndex();

        if (currentIndex <= 0)
            return null;

        Transform previousSibling = transform.parent.GetChild(currentIndex - 1);
        return previousSibling.GetComponent<MainState>();
    }

    private void InitShootingData()
    {
        IShootingTargetsData[] shootingData = GetComponentsInChildren<IShootingTargetsData>();

        foreach (var data in shootingData)
            data.InitShootingTargets(_enemies, _hostages);
    }

    private void PrepareManualRotation()
    {
        if (_agent == null)
            return;

        _previousAgentUpdateRotation = _agent.updateRotation;
        _agent.updateRotation = false;
    }

    private void RestoreAgentRotation()
    {
        if (_agent == null)
            return;

        _agent.updateRotation = _previousAgentUpdateRotation;
    }

    private void RotateToLookAtPoint()
    {
        if (_lookAtPoint == null || _rotator == null)
            return;

        _rotator.RotateToInstant(_lookAtPoint);
    }

    private bool IsNextStatePlayerShootingState()
    {
        if (transform.parent == null)
            return false;

        int nextIndex = transform.GetSiblingIndex() + 1;

        if (nextIndex >= transform.parent.childCount)
            return false;

        return transform.parent.GetChild(nextIndex).TryGetComponent(out PlayerShootingState state);
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

    private void HideEnemiesBeforeShooting()
    {
        int index = transform.GetSiblingIndex();

        if (index != 0)
        {
            Transform previousState = transform.parent.GetChild(index - 1);

            if (previousState.TryGetComponent(out MainState state))
            {
                ShowStateNextEnemies showStateNextEnemiesstate = gameObject.AddComponent<ShowStateNextEnemies>();
                showStateNextEnemiesstate.Init(_enemies, _hostages, _additionalObjects, state, true);
            }
        }

        if (index != transform.parent.childCount - 1)
        {
            Transform nextState = transform.parent.GetChild(index + 1);

            if (nextState.TryGetComponent(out MainState state))
            {
                ShowStateNextEnemies showStateNextEnemiesstate = gameObject.AddComponent<ShowStateNextEnemies>();
                showStateNextEnemiesstate.Init(_enemies, _hostages, _additionalObjects, state, false);
            }
        }
    }
}
