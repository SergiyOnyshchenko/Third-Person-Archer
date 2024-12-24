using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class HostageController : System, IActorIniter
    {
        [SerializeField] private bool _isJailed;
        private bool _isSaved;
        private Health _health;
        public bool IsSaved { get => _isSaved; }
        public bool IsJailed { get => _isJailed; }
        public bool IsDead { get => _health.IsDead; }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out Health health))
            {
                _health = health;
                SubscribeDeath();
            }
        }

        public void Save()
        {
            if (_isJailed)
                return;

            if (_isSaved)
                return;

            _isSaved = true;
        }

        public void Jail()
        {
            _isJailed = false;
            Save();
        }

        private void SubscribeDeath()
        {
            _health.OnHealthZero.AddListener(PunishPlayer);
        }

        private void UnsubscribeDeath()
        {
            _health.OnHealthZero.RemoveListener(PunishPlayer);
        }

        private void PunishPlayer()
        {
            UnsubscribeDeath();
            HostageEventSystem.SendHostageDied();

            if (AppMetricaEventReporter.Instance != null)
                AppMetricaEventReporter.Instance.SendHostageDied();
        }
    }
}