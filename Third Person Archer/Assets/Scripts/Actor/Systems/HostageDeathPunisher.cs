using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class HostageDeathPunisher : System, IActorIniter
    {
        private Health _health;
        public UnityEvent OnPunished = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out Health health))
            {
                _health = health;
            }
        }

        private void OnEnable()
        {
            HostageEventSystem.OnHostageDied.AddListener(Punish);
        }

        private void OnDisable()
        {
            HostageEventSystem.OnHostageDied.RemoveListener(Punish);
        }

        public void Punish()
        {
            _health.ApplyDamage(50);
            OnPunished?.Invoke();
        }
    }
}