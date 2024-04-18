using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SingleProperty<T> : Property
    {
        [SerializeField] protected T _value;
        public T Value { get => _value; }

        public void SetValue(T value)
        {
            _value = value;
        }
    }
}