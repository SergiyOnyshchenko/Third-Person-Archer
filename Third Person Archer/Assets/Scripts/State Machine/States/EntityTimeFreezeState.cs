using UnityEngine;

public class EntityTimeFreezeState : MainState
{
    [SerializeField] private Actor.Animator _animator;

    public override void Enter()
    {
        base.Enter();

        _animator.SetSpeed(0);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
