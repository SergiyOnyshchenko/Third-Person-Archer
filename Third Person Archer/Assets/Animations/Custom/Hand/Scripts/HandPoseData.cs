using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Hand
{
    [CreateAssetMenu(fileName = "HandPoseData", menuName = "Pose/HandPoseData")]
    public class HandPoseData : AnimationPoseData<FingerBoneData>{}   
}
