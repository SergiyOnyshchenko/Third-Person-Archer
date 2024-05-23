using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class DeathState : MainState, IActorIniter
    {
        private RagdollControll _ragdoll;

        public void InitActor(ActorController actor)
        {
            _ragdoll = actor.GetComponentInChildren<RagdollControll>();
        }

        public override void Enter()
        {
            base.Enter();

            if(_ragdoll != null)
                _ragdoll.MakePhysical();
        }
    }
}