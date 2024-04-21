using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class BodyPartIKSetter : MonoBehaviour
    {

        [SerializeField] private Transform _target;
        private BodyPartIK[] _bodyParts;
        private BipedIK _bipedIK;
        private CCDIK _headMoveIK;
        private TrigonometricIK _headRotateIK;

        private void Awake()
        {
            if(_target == null)
                _target = transform;
            
            _bipedIK = _target.GetComponent<BipedIK>();
            _headMoveIK = _target.GetComponent<CCDIK>();
            _headRotateIK = _target.GetComponent<TrigonometricIK>();
            _bodyParts = _target.GetComponentsInChildren<BodyPartIK>();

            SetBodyPivots();
            SetHeadPivots();
        }

        private void SetBodyPivots()
        {
            foreach (var bodyPart in _bodyParts)
            {
                switch (bodyPart.Name)
                {
                    case BodyNameIK.Pelvis:
                        _bipedIK.solvers.pelvis.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.Spine:
                        _bipedIK.solvers.spine.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.Head:
                        _headMoveIK.solver.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.LookAt:
                        _bipedIK.solvers.lookAt.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.LeftHand:
                        _bipedIK.solvers.leftHand.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.RightHand:
                        _bipedIK.solvers.rightHand.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.LeftFoot:
                        _bipedIK.solvers.leftFoot.target = bodyPart.IkPoint;
                        break;
                    case BodyNameIK.RightFoot:
                        _bipedIK.solvers.rightFoot.target = bodyPart.IkPoint;
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetHeadPivots()
        {
            var head = GetBodyPartIK(BodyNameIK.Head);

            _headMoveIK.solver.target = head.IkPoint;
            _headRotateIK.solver.target = head.IkPoint;
        }

        private BodyPartIK GetBodyPartIK(BodyNameIK name)
        {
            foreach (var bodyPart in _bodyParts)
                if(bodyPart.Name == name)
                    return bodyPart;

            return null;
        }
    }
}