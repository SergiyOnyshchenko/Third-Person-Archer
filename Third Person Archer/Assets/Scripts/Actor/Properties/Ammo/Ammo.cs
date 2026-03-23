using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Meta.Weapons;

namespace Actor.Properties
{
    public abstract class Ammo<W> : Property, IActorIniter, IAmmoCount, IAmmoUpgrade where W : WeaponController
    {
        [SerializeField] private int _baseCount;
        private int _maxCount;
        private int _currentCount;
        private IShootEvent _shootEvent;
        public int AmmoCount => _currentCount;
        public abstract WeaponClass WeaponType { get; }

        public UnityEvent OnAmmoModified = new UnityEvent();

        private void Awake()
        {
            SetMaxCount(0);
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out W controller))
            {
                _shootEvent = controller;
                _shootEvent.OnHitResult -= OnHitResult;
                _shootEvent.OnHitResult += OnHitResult;
            }
        }

        private void OnEnable()
        {
            if (_shootEvent != null)
            {
                _shootEvent.OnHitResult -= OnHitResult;
                _shootEvent.OnHitResult += OnHitResult;
            }
        }

        private void OnDisable()
        {
            if (_shootEvent != null)
            {
                _shootEvent.OnHitResult -= OnHitResult;
            }
        }

        private void OnHitResult(HitInfo hitInfo)
        {
            if (!hitInfo.HitTarget)
                Decrease();
        }

        public void SetMaxCount(int count)
        {
            _maxCount = count;
            ResetAmmoCount();
        }

        public void ResetAmmoCount()
        {
            _currentCount = _maxCount;
            OnAmmoModified?.Invoke();
        }

        public void Decrease()
        {
            Modify(-1);
        }

        public void Modify(int value)
        {
            _currentCount += value;

            if (_currentCount < 0)
                _currentCount = 0;

            OnAmmoModified?.Invoke();
        }
    }
}