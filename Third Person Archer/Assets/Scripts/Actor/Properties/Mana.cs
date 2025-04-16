using System.Collections;
using System.Collections.Generic;
using Playgama;
using UnityEngine;
using UnityEngine.Events;

namespace Actor.Properties
{
    public class Mana : Property
    {
        [SerializeField] private int _maxValue;
        private int _value;
        private float _multiplier = 1f;
        public int Value { get => _value; }
        public float Ratio { get => (float)_value / (float)_maxValue; }
        public UnityEvent<float> OnManaModified = new UnityEvent<float>();
        public UnityEvent OnManaFull = new UnityEvent();

        private void OnEnable()
        {
            Bridge.storage.Get("Mana", (success, value) =>
            {
                if (success && int.TryParse(value, out var result))
                {
                    _value = Mathf.Clamp(result, 0, _maxValue);
                }
                else
                {
                    _value = 0;
                }

                OnManaModified?.Invoke(Ratio);
            });
        }

        private void OnDisable()
        {
            Bridge.storage.Set("Mana", _value.ToString(), null);
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                _multiplier = LevelManager.Instance.CurrentLevel.ManaMultiplier;
            }
        }

        public void Add(int value)
        {
            _value += Mathf.RoundToInt( value * _multiplier );
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