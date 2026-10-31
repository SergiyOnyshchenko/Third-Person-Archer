using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class EnemyAttackState : MainState, IActorIniter
{
    [Tooltip("Wait for a turn from the AttackDirector before attacking, so enemies don't all fire at once.")]
    [SerializeField] private bool _useDirector = true;
    [Tooltip("Degrees per second the enemy keeps turning to the target while it waits for its turn.")]
    [SerializeField] private float _waitTurnSpeed = 180f;

    private ActorController _actor;
    private EnemyAttackInput _attackInput;
    private PerceptionInput _perception;
    private bool _waiting;

    protected override bool EnterChildrenOnEnter => false;

    public void InitActor(ActorController actor)
    {
        _actor = actor;

        if(actor.TryGetInput(out EnemyAttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;
    }

    public override void Enter()
    {
        base.Enter();

        _waiting = _useDirector && !AttackDirector.TryAcquire(_actor);
        if (!_waiting)
            BeginAttack();
    }

    public override void Exit()
    {
        if (_useDirector)
        {
            if (_waiting)
                AttackDirector.Cancel(_actor);
            else if (!AttackDirector.HasStarted(_actor))
                AttackDirector.Release(_actor);
        }

        _waiting = false;
        _attackInput.AllowAttack(false);
        base.Exit();
    }

    private void Update()
    {
        if (!_waiting)
            return;

        TurnToTarget();

        if (AttackDirector.TryAcquire(_actor))
        {
            _waiting = false;
            BeginAttack();
        }
    }

    private void BeginAttack()
    {
        // The state's own timers only start with the attack.
        EnterChildren();
        _attackInput.AllowAttack(true);
    }

    private void TurnToTarget()
    {
        if (_perception == null || _perception.Target == null)
            return;

        Transform self = _actor.transform;
        Vector3 direction = Vector3.ProjectOnPlane(_perception.Target.TargetPoint.position - self.position, Vector3.up);
        if (direction.sqrMagnitude < 0.01f)
            return;

        self.rotation = Quaternion.RotateTowards(self.rotation, Quaternion.LookRotation(direction), _waitTurnSpeed * Time.deltaTime);
    }
}
