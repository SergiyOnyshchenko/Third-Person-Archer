using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class BeginMovingTransition : StateTransition, IActorIniter
    {
        private IMoveInput _input;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out MoveInput input))
                _input = input;
        }

        private void Update()
        {
            if (_input == null)
                return;

            if (_input.IsMoving)
                DoTransition();
        }
    }
}

