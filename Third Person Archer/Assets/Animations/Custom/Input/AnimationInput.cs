using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Input
{
    public class AnimationInput : MonoBehaviour
    {
        private Joystick _joystick;
        private IActiveAnimation[] _animations;

        private void Awake()
        {
            _joystick = FindObjectOfType<Joystick>();
            _animations = GetComponentsInChildren<IActiveAnimation>();
        }

        private void Start()
        {
            foreach (var animation in _animations)
                animation.Play();
        }

        private void FixedUpdate()
        {
            foreach (var animation in _animations)
                animation.SetAnimationValue(_joystick.Horizontal);
        }
    }
}