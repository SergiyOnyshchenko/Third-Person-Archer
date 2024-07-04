using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class JoystickFpvInput : FpvInput
    {
        [SerializeField] private Joystick _joystick;
        [SerializeField] private Transform _joystickPivot;
        public float Distance { get; private set; } 

        private void Awake()
        {
            Distance = 0;
        }

        private void Update()
        {
            Horizontal = _joystick.Horizontal;
            Vertical = _joystick.Vertical;

            if (UnityEngine.Input.GetMouseButton(0))
            {
                Vector2 joystickPosition = new Vector2(
                    _joystickPivot.position.x / Screen.width - 0.5f,
                    _joystickPivot.position.y / Screen.height - 0.5f);

                Vector3 mousePosition = new Vector2(
                    UnityEngine.Input.mousePosition.x / Screen.width - 0.5f,
                    UnityEngine.Input.mousePosition.y / Screen.height - 0.5f);

                float distance = Vector3.Distance(mousePosition, joystickPosition);
                Distance = distance * 2;
            }
        }
    }
}
