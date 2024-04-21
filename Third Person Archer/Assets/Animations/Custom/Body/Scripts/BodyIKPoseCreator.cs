using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class BodyIKPoseCreator : AnimationPoseDataCreator<BodyPartIK, BodyPartIKData>
    {
        [ContextMenu("Create Pose Data")]
        public override void CreatePoseData() => base.CreatePoseData();

        protected override AnimationPoseData<BodyPartIKData> GetNewData()
        {
            ScriptableObject pose = ScriptableObject.CreateInstance(typeof(BodyIKPoseData));
            BodyIKPoseData data = (BodyIKPoseData)pose;
            data.Init(_animator.GetPoseData());
            return data;
        }
    }
}