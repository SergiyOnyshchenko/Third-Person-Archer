using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class JoystickFpvInput : FpvInput
    {
        [SerializeField] private Joystick _joystick;

        private void Awake()
        {
            
        }

        private void Update()
        {
            Horizontal = _joystick.Horizontal;
            Vertical = _joystick.Vertical;

            /*
            if (UnityEngine.Input.GetMouseButtonDown(0))
                Fire = true;

            if (UnityEngine.Input.GetMouseButtonUp(0))
                Fire = false;
            */
        }
    }
}
