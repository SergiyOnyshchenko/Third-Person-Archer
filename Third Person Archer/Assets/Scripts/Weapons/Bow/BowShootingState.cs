using Actor;

public class BowShootingState : ProcessState, IActorIniter
{
    private BowController _bowController;
    private AttackInput _attackInput;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out BowController bow))
            _bowController = bow;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        _bowController.SetStartSettings();
        _attackInput.OnAttackRelease.AddListener(PullArrow);
    }

    public override void Exit() 
    {
        _attackInput.OnAttackRelease.RemoveListener(PullArrow);
        base.Exit();
    }

    private void Update()
    {
        if (_attackInput.IsHold)
        {
            if(!_bowController.IsPulling)
                _bowController.BeginPull();

            _bowController.HoldPull();
        }
    }

    private void PullArrow()
    {
        _bowController.ReleasePull();
        FinishProcess();
    }
}
