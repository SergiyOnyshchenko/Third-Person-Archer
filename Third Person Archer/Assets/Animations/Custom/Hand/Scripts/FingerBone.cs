using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Hand
{
    public class FingerBone : ITransform, IFingerBone 
    {
        public Transform Transform { get; private set; }
        public FingerName FingerName  { get; private set; }
        public int Index  { get; private set; }
        public Quaternion Rotation => Transform.localRotation;
        
        public FingerBone(Transform transform, FingerName fingerName, int index)
        {
            Transform = transform;
            FingerName = fingerName;
            Index = index;
        }
    }
}