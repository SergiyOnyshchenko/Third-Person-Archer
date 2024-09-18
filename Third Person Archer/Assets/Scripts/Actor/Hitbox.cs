using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Actor
{
    public class Hitbox : MonoBehaviour, IDamageReciever, IDamageable, ITriggerReciever, IDamageChecker
    {
        [SerializeField, Range(0, 100)] private float _demageMultiplier = 1f;
        [SerializeField] private LayerMask _collisionMask;
        public event Action<int> OnDamaged;
        public event Action<string, GameObject> OnTriggered;
        public IDamageReciever.OnTryDamaged TryDamagedCallback { get; set; }

        public UnityEvent<int> OnDamagedEvent = new UnityEvent<int>();
        public UnityEvent<Collision> OnCollided = new UnityEvent<Collision>();

        public void ReciveTrigger(string name, GameObject owner) => OnTriggered?.Invoke(name, owner);

        public void DoDamage(int damage)
        {
            int calculatedDamage = Mathf.RoundToInt(damage * _demageMultiplier);
            OnDamaged?.Invoke(calculatedDamage);
            OnDamagedEvent?.Invoke(calculatedDamage);
        }

        public int GetHealthAfterDamage(int damage)
        {
            var health = TryDamagedCallback?.Invoke(Mathf.RoundToInt(damage * _demageMultiplier));

            if (health == null)
                return 10;
            else
                return (int)health;
        }

        public void SetCollisionMask(LayerMask collisionMask)
        {
            _collisionMask = collisionMask;
        }

        private void OnCollisionEnter(Collision collision)
        {
            /*
            if ((_collisionMask.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
            {
                OnCollided?.Invoke(collision);
            }
            */
        }
    }
}