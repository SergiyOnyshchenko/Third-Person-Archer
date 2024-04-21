using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Hand
{
    public interface IFingerBone 
    {
        FingerName FingerName { get; }
        int Index { get; }
        Quaternion Rotation { get; }  
    }
}
