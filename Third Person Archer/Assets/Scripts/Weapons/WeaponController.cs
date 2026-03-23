using System;
using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public abstract class WeaponController : System, IActorIniter, IShootEvent
    {
        protected IAmmoCount _ammoCount;
        private AimInput _aimInput;
        private Shooter _shooter;
        public event Action OnShooted;
        public event Action<HitInfo> OnHitResult;

        private void Start()
        {
            _shooter = GetComponent<Shooter>();
        }

        public virtual void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out AimInput aimInput))
                _aimInput = aimInput;
        }

        public virtual bool CanAttack()
        {
            if (_ammoCount.AmmoCount <= 0)
                return false;

            return true;
        }

        protected void Shoot(float multiplier, UnityAction targetHited)
        {
            ActorController hitActor = null;
            bool hitTarget = false;
            
            void OnTargetHit(ActorController actor)
            {
                hitTarget = true;
                hitActor = actor;
                targetHited?.Invoke();
            }

            void OnAnyHit()
            {
                OnHitResult?.Invoke(new HitInfo(hitTarget, hitActor));
            }

            _shooter.Shoot(_aimInput.GetAimDirection(), multiplier, OnTargetHit, OnAnyHit);
        }
    }
}