using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace Actor
{
    public class Animator : System, IAnimator
    {
        [SerializeField] private UnityEngine.Animator _animator;
        [SerializeField] private AnimationEventReciever _aniamtionEventReciever;
        [Space]
        [SerializeField] private AnimationEvent[] _events;

        private void OnEnable()
        {
            if(_aniamtionEventReciever != null)
                _aniamtionEventReciever.OnAnimationEvent.AddListener(TryInvokeAnimationEvent);
        }

        private void OnDisable()
        {
            if (_aniamtionEventReciever != null)
                _aniamtionEventReciever.OnAnimationEvent.RemoveListener(TryInvokeAnimationEvent);
        }

        public void SetAnimator(UnityEngine.Animator animator)
        {
            _animator = animator;
        }

        public void SwapAnimatorController(RuntimeAnimatorController animatorController)
        {
            _animator.runtimeAnimatorController = animatorController;
        }

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
        public bool TryGetAnimationEvent(string name, out AnimationEvent animEvent)
        {
            foreach (var myEvent in _events)
            {
                if(myEvent.Name == name)
                {
                    animEvent = myEvent; 
                    return true;
                }
            }

            animEvent = null;
            return false;
        }

        private void TryInvokeAnimationEvent(string name)
        {
            foreach (var myEvent in _events)
            {
                myEvent.TryInvoke(name);
            }
        }

    }
}