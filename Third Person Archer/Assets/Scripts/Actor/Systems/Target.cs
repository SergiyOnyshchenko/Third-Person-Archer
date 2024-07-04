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
        public IDamageReciever.OnTryDamaged TryDamagedCallback { get; set; }

        public event Action<ITarget> OnDied;

        public void InitActor(ActorController actor)
        {
            RootPoint = actor.transform;

            if (actor.TryGetSystem(out Health health))
            {
                _health = health;
                _health.OnHealthZero.AddListener(Die);
            }
        }

        public void DoDamage(int damage)
        {
            OnDamaged?.Invoke(damage);
        }

        private void Die()
        {
            _health.OnHealthZero.RemoveListener(Die);
            OnDied?.Invoke(this);
        }
    }
}