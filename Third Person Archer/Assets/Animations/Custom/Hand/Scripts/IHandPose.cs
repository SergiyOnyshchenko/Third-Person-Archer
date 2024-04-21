using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Hand
{
    public interface IHandPose 
    {
        IFingerBone[] FingerBones { get; }
    }
}