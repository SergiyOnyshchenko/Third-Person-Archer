using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{

[System.Serializable]
public class BodyIK
{
    [field: SerializeField] public BodyPartIK Pelvis { get; private set; }
    [field: SerializeField] public BodyPartIK Spine { get; private set; }
    [field: SerializeField] public BodyPartIK Head { get; private set; }
    [field: SerializeField] public BodyPartIK LookAt { get; private set; }
    [field: SerializeField] public BodyPartIK LeftHand { get; private set; }
    [field: SerializeField] public BodyPartIK RightHand { get; private set; }
    [field: SerializeField] public BodyPartIK LeftFoot { get; private set; }
    [field: SerializeField] public BodyPartIK RightFoot { get; private set; }

    public BodyPartIK[] GetBodyParts() => new BodyPartIK[] { Pelvis, Spine, Head, LookAt, LeftHand, RightHand, LeftFoot, RightFoot };

    public BodyPartIK GetBodyPart(BodyNameIK pivotName)
    {
        switch (pivotName)
        {
            case BodyNameIK.Pelvis:
                return Pelvis;
            case BodyNameIK.Spine:
                return Spine;
            case BodyNameIK.Head:
                return Head;
            case BodyNameIK.LookAt:
                return LookAt;
            case BodyNameIK.LeftHand:
                return LeftHand;
            case BodyNameIK.RightHand:
                return RightHand;
            case BodyNameIK.LeftFoot:
                return LeftFoot;
            case BodyNameIK.RightFoot:
                return RightFoot;
        }
        return null;
    }

    public BodyPartIKData[] GetBodyPartsData() => new BodyPartIKData[] 
    { 
        new BodyPartIKData(BodyNameIK.Pelvis, Pelvis.transform), 
        new BodyPartIKData(BodyNameIK.Spine, Spine.transform), 
        new BodyPartIKData(BodyNameIK.Head, Head.transform), 
        new BodyPartIKData(BodyNameIK.LookAt, LookAt.transform), 
        new BodyPartIKData(BodyNameIK.LeftHand, LeftHand.transform),
        new BodyPartIKData(BodyNameIK.RightHand, RightHand.transform),
        new BodyPartIKData(BodyNameIK.LeftFoot, LeftFoot.transform),
        new BodyPartIKData(BodyNameIK.RightFoot, RightFoot.transform)
    };
}

}
