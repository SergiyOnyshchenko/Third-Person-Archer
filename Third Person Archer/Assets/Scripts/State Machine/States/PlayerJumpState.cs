using Actor;
using DG.Tweening;
using UnityEngine;

public class PlayerJumpState : MainState, IActorIniter
{
    [SerializeField] private Spline _jumpSpline;
    [SerializeField] private JumpType _jumpType;
    private float _delay = 0.25f;
    private JumpInput _jumper;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out JumpInput jumper))
            _jumper = jumper;
    }

    public override void Enter()
    {
        base.Enter();

        DOVirtual.DelayedCall(_delay, () => _jumper.Jump(_jumpSpline, _jumpType));
    }
}
