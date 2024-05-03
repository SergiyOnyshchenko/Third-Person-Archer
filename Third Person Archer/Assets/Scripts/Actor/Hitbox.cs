using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Actor
{
    public class Hitbox : MonoBehaviour, IDamageReciever, IDamageable
    {
        [SerializeField, Range(0, 100)] private float _demageMultiplier = 1f;
        [SerializeField] private LayerMask _collisionMask;
        public event Action<int> OnDamaged;
        public UnityEvent<Collision> OnCollided = new UnityEvent<Collision>();

        public void DoDamage(int damage)
        {
            int calculatedDamage = Mathf.RoundToInt(damage * _demageMultiplier);
            OnDamaged?.Invoke(calculatedDamage);
        }

        public void SetCollisionMask(LayerMask collisionMask)
        {
            _collisionMask = collisionMask;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((_collisionMask.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
            {
                OnCollided?.Invoke(collision);
            } 
        }
    }
}