using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class JumpInput : Input
    {
        public Spline JumpSpline { get; private set; }
        public UnityEvent OnBeginJump = new UnityEvent();

        private void Start()
        {
            IsActive = true;
        }

        public void Jump(Spline spline)
        {
            if (!IsActive)
                return;

            JumpSpline = spline;
            OnBeginJump?.Invoke();

            Debug.Log("Jump Input");
        }
    }
}
