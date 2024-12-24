using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Actor
{
    public class DeathState : MainState, IActorIniter
    {
        [SerializeField] private RagdollControll _ragdoll;
        private ActorController _actor;

        public void InitActor(ActorController actor)
        {
            _actor = actor;
            _ragdoll = actor.GetComponentInChildren<RagdollControll>();
        }

        public override void Enter()
        {
            base.Enter();

            _actor.DeathHandler();

            if (_ragdoll != null)
                _ragdoll.MakePhysical();
        }
    }
}