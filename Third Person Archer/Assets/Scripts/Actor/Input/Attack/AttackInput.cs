using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class AttackInput : Input
    {
        [SerializeField] private bool _allowDebug;
        protected bool _isHold;
        public bool IsHold { get => _isHold; }
        public UnityEvent OnAttackRelease = new UnityEvent();

        public void AllowAttack(bool value)
        {
            IsActive = value;
            _allowDebug = IsActive;
        }

        protected void SendAttackRelease()
        {
            OnAttackRelease?.Invoke();
        }
    }
}
