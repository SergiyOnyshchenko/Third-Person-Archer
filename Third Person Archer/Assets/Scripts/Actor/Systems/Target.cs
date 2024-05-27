using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Target : System, IActorIniter, ITarget, IDamageReciever, IDamageable
    {
        private Health _health;
        public Transform RootPoint { get; private set; }
        [field: SerializeField] public Transform TargetPoint { get; private set; } 
        public event Action<int> OnDamaged;
        public bool IsDead { get => _health.IsDead; }
        public IDamageable Damageable => this;

        public void InitActor(ActorController actor)
        {
            RootPoint = actor.transform;

            if (actor.TryGetSystem(out Health health))
                _health = health;
        }

        public void DoDamage(int damage)
        {
            OnDamaged?.Invoke(damage);
        }
    }
}