using System.Collections;
using System.Collections.Generic;
using CustomAnimation.Body;
using UnityEngine;
using static RootMotion.FinalIK.Grounding;
using static RootMotion.FinalIK.IKSolverVR;

namespace CustomAnimation.FPV
{
    [System.Serializable]
    public class FpvIK
    {
        [field: SerializeField] public BodyPartIK LeftHand { get; private set; }
        [field: SerializeField] public BodyPartIK RightHand { get; private set; }
        public BodyPartIK[] GetBodyParts() => new BodyPartIK[] { LeftHand, RightHand};

        public BodyPartIK GetBodyPart(BodyNameIK pivotName)
        {
            switch (pivotName)
            {
                case BodyNameIK.LeftHand:
                    return LeftHand;
                case BodyNameIK.RightHand:
                    return RightHand;
            }
            return null;
        }

        public BodyPartIKData[] GetBodyPartsData() => new BodyPartIKData[]
        {
            new BodyPartIKData(BodyNameIK.LeftHand, LeftHand.transform),
            new BodyPartIKData(BodyNameIK.RightHand, RightHand.transform)
        };
    }
}
