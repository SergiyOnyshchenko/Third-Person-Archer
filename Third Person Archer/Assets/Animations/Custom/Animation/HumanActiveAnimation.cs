using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.Body;
using CustomAnimation.Hand;
using System;
using UnityEditor;

[Serializable]
public class HumanActiveAnimation : MonoBehaviour, IActiveAnimation 
{
    [SerializeField] private float _springPower;
    [SerializeField] private float _springDumping;
    [Space]
    [SerializeField] private ActiveBodyIKAnimation _bodyAnimation;
    [SerializeField] private ActiveHandAnimation _leftHandAnimation;
    [SerializeField] private ActiveHandAnimation _rightHandAnimation;
    public Dictionary<IActiveAnimation, AnimatorType> Animations;
    private SpringFloat _spring;

    private void Awake()
    {
        Animations = new Dictionary<IActiveAnimation, AnimatorType>() 
        {
            {_bodyAnimation, _bodyAnimation.AnimatorType},
            {_leftHandAnimation, _leftHandAnimation.AnimatorType},
            {_rightHandAnimation, _rightHandAnimation.AnimatorType}
        };

        _spring = new SpringFloat(_springPower, _springDumping);
    }

    private void Start() 
    {
        Play();
    }

    public void Play()
    {
        foreach (var animation in Animations)
            animation.Key.Play();
    }

    public void Stop()
    {
        foreach (var animation in Animations)
            animation.Key.Stop();
    }

    public void SetAnimationValue(float value)
    {
        _spring.UpdateValue(value);

        float springValue = _spring.Value;

        foreach (var animation in Animations)
        {
            if(animation.Value == AnimatorType.Spring)
                animation.Key.SetAnimationValue(value);
            else
                animation.Key.SetAnimationValue(springValue);
        }
    }
}
