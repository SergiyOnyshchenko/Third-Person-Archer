using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Target : System, IActorIniter, ITarget
    {
        [field: SerializeField] public Transform TargetPoint { get; private set; }
        private Health _health;
        public bool IsDead { get => _health.IsDead; }

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetSystem(out Health health))
                _health = health;
        }
    }
}