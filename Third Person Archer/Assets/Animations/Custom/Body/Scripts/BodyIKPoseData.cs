using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    [CreateAssetMenu(fileName = "BodyIKPoseData", menuName = "Pose/BodyIKPoseData")]
    public class BodyIKPoseData : AnimationPoseData<BodyPartIKData>{}
}
