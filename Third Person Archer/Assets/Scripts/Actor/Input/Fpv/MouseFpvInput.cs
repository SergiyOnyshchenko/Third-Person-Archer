using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Actor
{
    public class MouseFpvInput : FpvInput
    {
        [SerializeField] private float _mouseSensitivity = 100f;

        float xRotation = 0f;

        void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            Horizontal = UnityEngine.Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            Vertical = UnityEngine.Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        }
    }
}