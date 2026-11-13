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
            if (_aniamtionEventReciever != null)
                _aniamtionEventReciever.OnAnimationEvent.AddListener(TryInvokeAnimationEvent);
        }

        private void OnDisable()
        {
            if (_aniamtionEventReciever != null)
                _aniamtionEventReciever.OnAnimationEvent.RemoveListener(TryInvokeAnimationEvent);
        }

        public void Enable(bool value)
        {
            _animator.enabled = value;
        }

        public void SetAnimator(UnityEngine.Animator animator)
        {
            _animator = animator;
        }

        public void SetEventReciever(AnimationEventReciever eventReciever)
        {
            _aniamtionEventReciever = eventReciever;
        }

        public void SwapAnimatorController(RuntimeAnimatorController animatorController)
        {
            _animator.runtimeAnimatorController = animatorController;
        }

        public void SetSpeed(float speed)
        {
            _animator.speed = speed;
        }

        public void SetBool(string name, bool value)
        {
            _animator.SetBool(name, value);
        }

        public void SetFloat(string name, float value)
        {
            _animator.SetFloat(name, value);
        }

        public bool HasState(string stateName, int layer = 0)
        {
            return _animator != null && _animator.HasState(layer, UnityEngine.Animator.StringToHash(stateName));
        }

        /// <summary>Blends into the state (duration in seconds), from its start or <paramref name="timeOffset"/> seconds into it.</summary>
        public void CrossFade(string stateName, float duration, int layer = 0, float timeOffset = 0f)
        {
            _animator.CrossFadeInFixedTime(stateName, duration, layer, timeOffset);
        }

        /// <summary>True while the layer is in <paramref name="stateName"/> or blending into it.</summary>
        public bool IsInState(string stateName, int layer = 0)
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
                return false;

            if (_animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                return true;

            return _animator.IsInTransition(layer) && _animator.GetNextAnimatorStateInfo(layer).IsName(stateName);
        }

        /// <summary>Switches to the state instantly at the given normalized time.</summary>
        public void Play(string stateName, float normalizedTime, int layer = 0)
        {
            _animator.Play(stateName, layer, normalizedTime);
        }

        /// <summary>Normalized time (0..1, looped) of the current state if it is <paramref name="stateName"/>; -1 otherwise.</summary>
        public float GetStatePhase(string stateName, int layer = 0)
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
                return -1f;

            var info = _animator.GetCurrentAnimatorStateInfo(layer);
            return info.IsName(stateName) ? Mathf.Repeat(info.normalizedTime, 1f) : -1f;
        }

        public float GetFloat(string name)
        {
            return _animator != null && _animator.isActiveAndEnabled ? _animator.GetFloat(name) : 0f;
        }

        public void SetInteger(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        public void SetTrigger(string name)
        {
            _animator.SetTrigger(name);
        }

        public void ResetTrigger(string name)
        {
            _animator.ResetTrigger(name);
        }
        
        public bool TryGetAnimationEvent(string name, out AnimationEvent animEvent)
        {
            foreach (var myEvent in _events)
            {
                if (myEvent.Name == name)
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