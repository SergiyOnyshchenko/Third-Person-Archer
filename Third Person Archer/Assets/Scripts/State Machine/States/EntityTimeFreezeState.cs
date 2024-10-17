using Actor;
using MoreMountains.Feedbacks;
using UnityEngine;

public class EntityTimeFreezeState : MainState, IActorIniter
{
    private Actor.Animator _animator;
    private MMWiggle _mmfPlayer;

    public override void Enter()
    {
        base.Enter();

        _animator.SetSpeed(0);
        if (_mmfPlayer != null)
        {
            _mmfPlayer.PositionActive = false;
            _mmfPlayer.RotationActive = false;
            _mmfPlayer.ScaleActive = false;
        }
    }

    public override void Exit()
    {
        _animator.SetSpeed(1);

        base.Exit();
    }

    public void InitActor(ActorController actor)
    {
        actor.TryGetSystem(out _animator);

        _mmfPlayer = actor.GetComponentInParent<MMWiggle>();
    }
}
