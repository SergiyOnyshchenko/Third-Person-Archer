using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

namespace Actor
{
    public class Health : System, IActorIniter
    {
        private MaxHealth _maxHealth;
        private int _health;
        private IDamageReciever[] _damageRecievers;
        public int Value { get => _health; }
        public bool IsDead { get => _health <= 0; }
        public UnityEvent<int, int> OnHealthModified = new UnityEvent<int, int>();
        public UnityEvent OnHealthZero = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetProperty(out MaxHealth maxHealth))
                _maxHealth = maxHealth;

            InitCurrentHealth(_maxHealth);

            _damageRecievers = actor.GetComponentsInChildren<IDamageReciever>();
            SubscribeDamageRecievers();
        }

        private void InitCurrentHealth(MaxHealth maxHealth)
        {
            _health = _maxHealth.Value;
        }

        private void ApplyDamage(int damage)
        {
            _health -= damage;
            _health = Mathf.Clamp(_health, 0, _maxHealth.Value);

            OnHealthModified?.Invoke(_health, _maxHealth.Value);

            if (_health == 0)
            {
                OnHealthZero?.Invoke();
            }
        }

        private void SubscribeDamageRecievers()
        {
            foreach (var reciever in _damageRecievers)
                reciever.OnDamaged += ApplyDamage;
        }

        private void UnsubscribeDamageRecievers()
        {
            foreach (var reciever in _damageRecievers)
                reciever.OnDamaged -= ApplyDamage;
        }
    }
}