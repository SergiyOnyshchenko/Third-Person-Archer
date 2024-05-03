using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class ActivatePerceptionSubstate : SubState, IActorIniter
    {
        private PerceptionInput _input;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out PerceptionInput input))
                _input = input;
        }

        public override void Enter()
        {
            base.Enter();
            _input.ActivatePerception(true);
        }

        public override void Exit()
        {
            _input.ActivatePerception(false);
            base.Exit();
        }
    }
}

