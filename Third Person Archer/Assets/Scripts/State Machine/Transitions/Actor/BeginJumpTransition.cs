using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace Actor
{
    public class BeginJumpTransition : StateTransition, IActorIniter
    {
        [SerializeField] private JumpInput _input;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out JumpInput input))
                _input = input;
        }

        public override void Enter()
        {
            base.Enter();
            _input.OnBeginJump.AddListener(DoTransition);
        }

        public override void Exit()
        {
            _input.OnBeginJump.RemoveListener(DoTransition);
            base.Exit();
        }
    }
}
