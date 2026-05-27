using UnityEngine;
using Actor;
using DG.Tweening;

public class PlayerCrouchSubstate : SubState, IActorIniter
{
    [SerializeField] private float _delay = 0.02f;
    private CrouchInput _input;
    private Tween _crouchDelayTween;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out _input)){}
    }

    public override void Enter()
    {
        base.Enter();

        _crouchDelayTween?.Kill();
        _crouchDelayTween = DOVirtual.DelayedCall(_delay, () => _input.StartCrouch()).SetTarget(this);
    }

    public override void Exit()
    {
        _crouchDelayTween?.Kill();
        _crouchDelayTween = null;

        _input.FinishCrouch();
        base.Exit();
    }
}
