using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.Body;
using CustomAnimation.Hand;

[System.Serializable]
public class AnimationPoseValue<T> where T : class
{   
    [SerializeField] private AnimationPoseData<T> _pose; 
    [SerializeField] private float _value;
    public AnimationPoseData<T> Pose { get => _pose; }
    public float Value { get => _value; }
}