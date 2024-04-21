using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class BodyIKPose : IAnimationPose<BodyPartIKData>
    {
        public BodyPartIKData[] Instances { get; set; }
    }
}