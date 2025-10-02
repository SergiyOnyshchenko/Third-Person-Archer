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
        [SerializeField] private int _health;
        private IDamageReciever[] _damageRecievers;
        public int Value { get => _health; }
        public float Ratio { get => _health / _maxHealth.Value; }
        public bool IsDead { get => _health <= 0; }
        public UnityEvent<int> OnDamaged = new UnityEvent<int>();
        public UnityEvent<int, int> OnHealthModified = new UnityEvent<int, int>();
        public UnityEvent OnHealthZero = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetProperty(out MaxHealth maxHealth))
            {
                _maxHealth = maxHealth;
                _maxHealth.OnPropertyChanged += UpdateMaxHealth;
            }

            InitCurrentHealth(_maxHealth);

            _damageRecievers = actor.GetComponentsInChildren<IDamageReciever>();
            SubscribeDamageRecievers();
        }

        private void OnDestroy()
        {
            if(_maxHealth != null)
                _maxHealth.OnPropertyChanged -= UpdateMaxHealth;
        }

        public void ApplyDamage(int damage)
        {
            if (_health <= 0)
                return;

            _health -= damage;
            _health = Mathf.Clamp(_health, 0, _maxHealth.Value);

            OnHealthModified?.Invoke(_health, _maxHealth.Value);
            OnDamaged?.Invoke(damage);

            if (_health == 0)
                OnHealthZero?.Invoke();
        }

        public void Die()
        {
            ApplyDamage(_health);
        }

        public void Revive()
        {
            InitCurrentHealth(_maxHealth);
            ApplyDamage(0);
        }

        private int TryApplyDamage(int damage)
        {
            int health = _health;
            health -= damage;
            return health;
        }

        private void UpdateMaxHealth()
        {
            InitCurrentHealth(_maxHealth);
        }

        private void InitCurrentHealth(MaxHealth maxHealth)
        {
            _health = _maxHealth.Value;
        }

        private void SubscribeDamageRecievers()
        {
            foreach (var reciever in _damageRecievers)
            {
                reciever.OnDamaged += ApplyDamage;
                reciever.TryDamagedCallback += TryApplyDamage;
            }
        }

        private void UnsubscribeDamageRecievers()
        {
            foreach (var reciever in _damageRecievers)
            {
                reciever.OnDamaged -= ApplyDamage;
                reciever.TryDamagedCallback -= TryApplyDamage;
            }
        }
    }
}