using UnityEngine;

namespace Actor
{
    public class BeginWallRunTransition : StateTransition, IActorIniter
    {
        [SerializeField] private WallRunInput _input;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out WallRunInput input))
                _input = input;
        }

        public override void Enter()
        {
            base.Enter();
            _input.OnBeginWallRun.AddListener(DoTransition);
        }

        public override void Exit()
        {
            _input.OnBeginWallRun.RemoveListener(DoTransition);
            base.Exit();
        }
    }
}