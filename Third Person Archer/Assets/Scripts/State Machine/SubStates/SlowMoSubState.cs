using Actor;
using UnityEngine.Events;

public class SlowMoSubState : SubState, IActorIniter
{
    private AttackInput _attackInput;
    public UnityEvent OnSlowMo = new UnityEvent();
    public UnityEvent OnReset = new UnityEvent();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AttackInput attackInput))
        {
            _attackInput = attackInput;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _attackInput.OnAttackStart.AddListener(DoSlowMo);
        _attackInput.OnAttackRelease.AddListener(ResetSlowMo);
    }

    public override void Exit() 
    {
        _attackInput.OnAttackStart.RemoveListener(DoSlowMo);
        _attackInput.OnAttackRelease.RemoveListener(ResetSlowMo);
        ResetSlowMo();
        /*ResetSlowMo();*/
        base.Exit();
    }

    public void DoSlowMo()
    {
        OnSlowMo?.Invoke();
    }

    public void ResetSlowMo()
    {
        OnReset?.Invoke();
    }
}
