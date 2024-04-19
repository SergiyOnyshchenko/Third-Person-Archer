using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Animator : System, IAnimator
    {
        [SerializeField] private UnityEngine.Animator _animator;

        public void SetBool(string name, bool value)
        {
            _animator.SetBool(name, value);
        }

        public void SetFloat(string name, float value)
        {
            _animator.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        public void SetTrigger(string name)
        {
            _animator.SetTrigger(name);
        }
    }
}