using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

namespace CustomAnimation.Body
{
    public class BodyIKSettingsSetter : MonoBehaviour
    {
        [Header("Bones")]
        [SerializeField] private Transform[] _headBones;
        [Header("IK References")]
        [SerializeField] private BipedIK _bipedBodyIK;
        [SerializeField] private CCDIK _headPositionIK;
        [SerializeField] private TrigonometricIK _headRotateIK;
        
        [ContextMenu("Apply Settings")]
        public void ApplySettings()
        {
            SetBodyBipedIKSettings();
            SetHeadCCDIKIKSettings();
            SetHeadTrigonometricIKSettings();
        }

        public void SetBodyBipedIKSettings()
        {
            SetFootSettings();
            SetLookAtSettings();
        }

        private void SetFootSettings()
        {
            var leftFoot = _bipedBodyIK.solvers.leftFoot;
            leftFoot.bendModifier = IKSolverLimb.BendModifier.Target;

            var rightFoot = _bipedBodyIK.solvers.rightFoot;
            rightFoot.bendModifier = IKSolverLimb.BendModifier.Target;
        }

        private void SetLookAtSettings()
        {       
            var lookAt = _bipedBodyIK.solvers.lookAt;
            lookAt.IKPositionWeight = 1f;
            lookAt.bodyWeight = 0f;
            lookAt.headWeight = 0f;
            lookAt.eyesWeight = 1f;
            lookAt.clampWeight = 0.5f;
            lookAt.clampWeightHead = 0.7f;
            lookAt.clampWeightEyes = 0.85f;
            lookAt.clampSmoothing = 2;
        }

        public void SetHeadCCDIKIKSettings()
        {
            _headPositionIK.solver.IKPositionWeight = 1f;
            _headPositionIK.solver.useRotationLimits = true;

            _headPositionIK.solver.bones = new IKSolver.Bone[_headBones.Length];


            for (int i = 0; i < _headBones.Length; i++)
            {
                _headPositionIK.solver.bones[i] = new IKSolver.Bone(_headBones[i]);
                //_headPositionIK.solver.bones[i].transform = _headBones[i];
            }

            _headPositionIK.solver.bones[0].weight = 0.25f;
            _headPositionIK.solver.bones[1].weight = 0.625f;
            _headPositionIK.solver.bones[2].weight = 1f;
        }

        public void SetHeadTrigonometricIKSettings()
        {
            _headRotateIK.solver.bone1.transform = _headBones[0];
            _headRotateIK.solver.bone2.transform = _headBones[1];
            _headRotateIK.solver.bone3.transform = _headBones[2];

            _headRotateIK.solver.IKPositionWeight = 0f;
            _headRotateIK.solver.IKRotationWeight = 1f;
            _headRotateIK.solver.bendNormal = new Vector3(1, 0, 0);
        }
    }
}
