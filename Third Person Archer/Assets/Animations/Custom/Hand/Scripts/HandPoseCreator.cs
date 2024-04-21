using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CustomAnimation.Hand
{
    public class HandPoseCreator : AnimationPoseDataCreator<FingerBone, FingerBoneData>
    {
        [ContextMenu("Create Pose Data")]
        public override void CreatePoseData() => base.CreatePoseData();

        protected override AnimationPoseData<FingerBoneData> GetNewData()
        {
            ScriptableObject pose = ScriptableObject.CreateInstance(typeof(HandPoseData));
            HandPoseData data = (HandPoseData)pose;
            data.Init(_animator.GetPoseData());
            return data;
        }
    }
}
