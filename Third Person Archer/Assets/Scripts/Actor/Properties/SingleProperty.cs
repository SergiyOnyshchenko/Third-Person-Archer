using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SingleProperty<T> : Property
    {
        [SerializeField] protected T _value;
        protected T _currentValue;
        public T Value { get => _currentValue; }
        public T BaseValue { get => _value; }
        public event Action OnPropertyChanged;

        private void Awake()
        {
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