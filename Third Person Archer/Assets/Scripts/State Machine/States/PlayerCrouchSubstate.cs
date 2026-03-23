using UnityEngine;
using Actor;
using DG.Tweening;

public class PlayerCrouchSubstate : SubState, IActorIniter
{
    [SerializeField] private float _delay = 0.02f;
    private CrouchInput _input;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out _input)){}
    }

    public override void Enter()
    {
        base.Enter();
        DOVirtual.DelayedCall(_delay, () => _input.StartCrouch() );
    }

    public override void Exit()
    {
        _input.FinishCrouch();
        base.Exit();
    }
}