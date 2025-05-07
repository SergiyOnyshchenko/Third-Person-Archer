using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

namespace Actor.Properties
{
    public abstract class Ammo<W> : Property, IActorIniter, IAmmoCount, IAmmoUpgrade where W : WeaponController
    {
        [SerializeField] private int _baseCount;
        private int _maxCount;
        private int _currentCount;
        private IShootEvent _shootEvent;
        public int AmmoCount => _currentCount;
        public abstract WeaponType WeaponType { get; }

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
                _shootEvent.OnShooted -= Decrease;
                _shootEvent.OnShooted += Decrease;
            }
        }

        private void OnEnable()
        {
            if (_shootEvent != null)
            {
                _shootEvent.OnShooted -= Decrease;
                _shootEvent.OnShooted += Decrease;
            }
        }

        private void OnDisable()
        {
            if (_shootEvent != null)
            {
                _shootEvent.OnShooted -= Decrease;
            }
        }

        public void SetMaxCount(int add)
        {
            _maxCount = _baseCount + add;
            ResetAmmoCount();
        }

        public void ResetAmmoCount()
        {
            _currentCount = _maxCount;
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