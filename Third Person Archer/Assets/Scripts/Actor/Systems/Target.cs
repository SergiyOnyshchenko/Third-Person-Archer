using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Target : System, IActorIniter, ITarget
    {
        public Transform RootPoint { get; private set; }
        [field: SerializeField] public Transform TargetPoint { get; private set; }
        private Health _health;
        public bool IsDead { get => _health.IsDead; }

        public void InitActor(ActorController actor)
        {
            RootPoint = actor.transform;

            if (actor.TryGetSystem(out Health health))
                _health = health;
        }
    }
}