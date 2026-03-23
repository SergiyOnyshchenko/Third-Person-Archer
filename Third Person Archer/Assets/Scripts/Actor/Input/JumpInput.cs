using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public enum JumpType
    {
        Regular,
        Roll
    }

    public class JumpInput : Input
    {
        public Spline JumpSpline { get; private set; }
        public JumpType Type { get; private set; }
        public UnityEvent OnBeginJump = new UnityEvent();

        private void Start()
        {
            IsActive = true;
            Type = JumpType.Regular;
        }

        public void Jump(Spline spline)
        {
            Jump(spline, JumpType.Regular);
        }

        public void Jump(Spline spline, JumpType type)
        {
            if (!IsActive)
                return;

            JumpSpline = spline;
            Type = type;
            OnBeginJump?.Invoke();
        }
    }
}
