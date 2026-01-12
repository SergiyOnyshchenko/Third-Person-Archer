using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actor.Properties
{
    public class SingleProperty<T> : Property, IActorIniter
    {
        [SerializeField, FormerlySerializedAs("Value")] protected T _currentValue;
        [SerializeField, FormerlySerializedAs("Base Value")] protected T _value;
        [SerializeField] private bool _applyBaseOnInit = true;
        public T Value { get => _currentValue; }
        public T BaseValue { get => _value; }
        public event Action OnPropertyChanged;

        public virtual void InitActor(ActorController actor)
        {
            if (!_applyBaseOnInit) return;

            if(_value == null) return;
                
            SetValue(_value);
        }

        public void SetValue(T value)
        {
            _currentValue = value;
            OnPropertyChanged?.Invoke();
        }

        public void ResetValue()
        {
            _currentValue = _value;
            OnPropertyChanged?.Invoke();
        }
    }
}