using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor.Properties
{
    public class Mana : Property
    {
        [SerializeField] private int _maxValue;
        private int _value;
        public int Value { get => _value; }
        public float Ratio { get => (float)_value / (float)_maxValue; }
        public UnityEvent<float> OnManaModified = new UnityEvent<float>();
        public UnityEvent OnManaFull = new UnityEvent();

        private void OnEnable()
        {
            _value = PlayerPrefs.GetInt("Mana", 0);
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt("Mana", _value);
        }

        public void Add(int value)
        {
            _value += value;
            _value = Mathf.Clamp(_value, 0, _maxValue);
            OnManaModified?.Invoke(Ratio);
        }

        public void Reset()
        {
            _value = 0;
            OnManaModified?.Invoke(Ratio);
        }
    }
}