using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CustomAnimation.Input
{
    public class AnimationInputVelocity : MonoBehaviour, IActiveAnimation
    {
        [SerializeField] private float _springPower;
        [SerializeField] private float _springDumping;
        private SpringFloat _spring;
        public UnityEvent<float> Action = new UnityEvent<float>();
        private float[] _pastLength = new float[50];
        private int _currentIndex;
        private float _pastValue;

        private void Awake()
        {
            ResetPastLength();
        }

        public void Play()
        {
            _spring = new SpringFloat(_springPower, _springDumping);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                ResetPastLength();
            }
        }

        public void SetAnimationValue(float value)
        {
            _spring.UpdateValue(value);

            float length = Mathf.Abs(value - _pastValue);
            _pastLength[_currentIndex] = length;

            _currentIndex++;

            if (_currentIndex >= _pastLength.Length)
                _currentIndex = 0;

            _pastValue = value;

            float sum = 0;
            for (int i = 0; i < _pastLength.Length; i++)
            {
                sum += _pastLength[i];
            }

            //Debug.Log(sum);

            CheckAction(value, sum);
        }

        public void Stop()
        {

        }

        private float _targetValue = 0.1f;
        private bool _activated = false;

        private void CheckAction(float value, float velocity)
        {
            velocity = Mathf.Abs(velocity);

            if (_activated)
            {
                if (value <= _targetValue)
                    _activated = false;

                return;
            }

            if (value > _targetValue)
            {
                _activated = true;
                float lerp = Mathf.InverseLerp(0, 6.5f, velocity);

                Debug.Log("ACTION --- " + lerp + " --- " + velocity);

                Action?.Invoke(lerp);
            }
        }

        private void ResetPastLength()
        {
            for (int i = 0; i < _pastLength.Length; i++)
            {
                _pastLength[i] = 0;
            }

            _currentIndex = 0;
        }
    }
}