using Actor;

public class BowShootingState : ProcessState, IActorIniter
{
    private BowController _bowController;
    private AttackInput _attackInput;
    private WeaponPull _weaponPull;
    private float _pullThreshold = 0.5f;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out BowController bow))
            _bowController = bow;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetProperty(out WeaponPull weaponPull))
            _weaponPull = weaponPull;
    }

    public override void Enter()
    {
        base.Enter();

        _bowController.SetStartSettings();

        _weaponPull.OnPullBegin.AddListener(_bowController.BeginPull);
        _attackInput.OnAttackRelease.AddListener(PullArrow);
    }

    public override void Exit() 
    {
        _weaponPull.OnPullBegin.RemoveListener(_bowController.BeginPull);
        _attackInput.OnAttackRelease.RemoveListener(PullArrow);
        base.Exit();
    }

    private void Update()
    {
        _bowController.HoldPull();
    }

    private void PullArrow()
    {
        if(_weaponPull.Value < _pullThreshold)
            return;

        _bowController.ReleasePull();
        FinishProcess();
    }
}
