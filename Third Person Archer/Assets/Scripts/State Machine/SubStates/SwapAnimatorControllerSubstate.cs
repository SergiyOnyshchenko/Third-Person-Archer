using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class SwapAnimatorControllerSubstate : SubState, IActorIniter
{
    [SerializeField] private RuntimeAnimatorController _newController;
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Actor.Animator animator))
        {
            _animator = animator;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _animator.SwapAnimatorController(_newController);
    }
}
