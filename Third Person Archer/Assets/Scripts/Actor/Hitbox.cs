using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Actor
{
    public class Hitbox : MonoBehaviour, IDamageReciever, IDamageable, ITriggerReciever, IDamageChecker, IActorIniter, IActorOwner
    {
        [SerializeField, Range(0, 2)] private float _demageMultiplier = 1f;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private bool _isTarget = true;
        private ActorController _actorHolder;
        public bool HasActor => _actorHolder;

        public ActorController ActorHolder => _actorHolder;
        public ActorController Actor => _actorHolder;
        public bool IsTarget => _isTarget;
        
        public event Action<int> OnDamaged;
        public event Action<string, GameObject> OnTriggered;
        public IDamageReciever.OnTryDamaged TryDamagedCallback { get; set; }
        public UnityEvent<int> OnDamagedEvent = new UnityEvent<int>();
        public UnityEvent<Collision> OnCollided = new UnityEvent<Collision>();

        public void InitActor(ActorController actor)
        {
            _actorHolder = actor;
        }

        public void ReciveTrigger(string name, GameObject owner) => OnTriggered?.Invoke(name, owner);

        public void DoDamage(int damage)
        {
            int calculatedDamage = Mathf.RoundToInt(damage * _demageMultiplier);
            OnDamaged?.Invoke(calculatedDamage);
            OnDamagedEvent?.Invoke(calculatedDamage);
        }

        public int GetHealthAfterDamage(int damage)
        {
            var health = TryDamagedCallback?.Invoke(Mathf.RoundToInt(damage));

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