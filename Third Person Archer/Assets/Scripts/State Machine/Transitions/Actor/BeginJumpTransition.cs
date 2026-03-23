using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace Actor
{
    public class BeginJumpTransition : StateTransition, IActorIniter
    {
        [SerializeField] private JumpInput _input;
        [SerializeField] private JumpType _type;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out JumpInput input))
                _input = input;
        }

        public override void Enter()
        {
            base.Enter();
            _input.OnBeginJump.AddListener(TryTransition);
        }

        public override void Exit()
        {
            _input.OnBeginJump.RemoveListener(TryTransition);
            base.Exit();
        }

        private void TryTransition()
        {
            if(_input.Type == _type)
                DoTransition();
        }
    }
}
