using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

namespace Actor.Properties
{
    public abstract class Ammo<W> : SingleProperty<int>, IActorIniter, IAmmoCount where W : WeaponController
    {
        private IShootEvent _shootEvent;
        public int AmmoCount => _value;
        public abstract WeaponType WeaponType { get; }

        public UnityEvent OnAmmoModified = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out W controller))
            {
                _shootEvent = controller;
                _shootEvent.OnShooted += Decrease;
            } 
        }

        private void OnEnable()
        {
            if(_shootEvent != null)
                _shootEvent.OnShooted += Decrease;
        }

        private void OnDisable()
        {
            if (_shootEvent != null)
                _shootEvent.OnShooted -= Decrease;
        }

        public void Decrease()
        {
            Modify(-1);
        }

        public void Modify(int value)
        {
            _value += value;

            if (_value < 0)
                _value = 0;

            OnAmmoModified?.Invoke();
        }
    }
}