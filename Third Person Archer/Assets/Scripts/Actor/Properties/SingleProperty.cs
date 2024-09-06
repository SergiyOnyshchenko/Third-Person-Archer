using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SingleProperty<T> : Property
    {
        [SerializeField] protected T _value;
        private T _baseValue;
        public T Value { get => _value; }
        public event Action OnPropertyChanged;

        private void Awake()
        {
            _baseValue = _value;
        }

        public void SetValue(T value)
        {
            _value = value;
            OnPropertyChanged?.Invoke();
        }

        public void ResetValue()
        {
            _value = _baseValue;
            OnPropertyChanged?.Invoke();
        }
    }
}