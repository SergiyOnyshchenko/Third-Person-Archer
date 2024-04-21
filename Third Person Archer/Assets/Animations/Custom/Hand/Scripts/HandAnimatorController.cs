using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using RootMotion.FinalIK;

namespace CustomAnimation.Hand
{
    public class HandAnimatorController : AnimatorController<FingerBone, FingerBoneData>
    {
        [Space]
        [SerializeField] private HandPoseData _startPose;
        [Space]
        [SerializeField] private bool _isRight;
        [SerializeField] private Transform _handParent;
        [SerializeField] private Finger[] _fingers;
        private FingerBone[] _fingerBones;

        protected override void Awake()
        {
            InitFingers();
            base.Awake();

            if (_startPose != null)
                DoPose(_startPose);
        }

        [ContextMenu("Set Fingers Manualy")]
        public void SetFingers()
        {
            _fingers = new Finger[5];

            for (int i = 0; i < _fingers.Length; i++)
            {
                _fingers[i] = new Finger();
                _fingers[i].SetFingerManualy((FingerName)i, _handParent.GetChild(i));
            }
        }

        public override void DoPose(IAnimationPose<FingerBoneData> pose)
        {
            FingerBoneData[] poseBonesData = pose.Instances;

            if(_isRight)
                foreach (var data in poseBonesData)
                    data.Inverce();

            for (int i = 0; i < _fingerBones.Length; i++)
                _currentAnimator.DoPose(_fingerBones[i], poseBonesData[i]);
        }

        public override IAnimationPose<FingerBoneData> LerpPoses(
            IAnimationPose<FingerBoneData> pose1,
            IAnimationPose<FingerBoneData> pose2,
            float value)
        {
            var newPose = new AnimationPose<FingerBoneData>();
            newPose.Instances = new FingerBoneData[pose1.Instances.Length];

            for (int i = 0; i < newPose.Instances.Length; i++)
            {
                var bodyPart1 = pose1.Instances[i];
                var bodyPart2 = pose2.Instances[i];

                Quaternion rotation = Quaternion.Lerp(bodyPart1.Rotation, bodyPart2.Rotation, value);
                newPose.Instances[i] = new FingerBoneData(bodyPart1.FingerName, bodyPart1.Index, rotation);
            }

            return newPose;
        }

        public override FingerBoneData[] GetPoseData()
        {
            List<FingerBoneData> allBonesData = new List<FingerBoneData>();

            for (int i = 0; i < _fingers.Length; i++)
            {
                Finger finger = _fingers[i];

                for (int j = 0; j < finger.Bones.Length; j++)
                {
                    FingerBone bone = finger.Bones[j];
                    allBonesData.Add(new FingerBoneData(finger.Name, j, bone.Rotation));
                }
            }

            return allBonesData.ToArray();
        }

        protected override AnimationProperty[] InitProperties()
        {
            List<AnimationProperty> properties = new List<AnimationProperty>();

            var duration = CreatePropertyObject("Duration").AddComponent<Duration>();
            duration.Init(1f);
            properties.Add(duration);

            var easeType = CreatePropertyObject("Ease").AddComponent<EaseType>();
            easeType.Init(DG.Tweening.Ease.OutBack);
            properties.Add(easeType);

            return properties.ToArray();
        }

        protected override Animator<FingerBone, FingerBoneData>[] InitAnimators()
        {
            List<Animator<FingerBone, FingerBoneData>> animators = new List<Animator<FingerBone, FingerBoneData>>();

            var instant = CreateAnimatorObject("Instant").AddComponent<HandInstantAnimator>();
            animators.Add(instant);

            if (_properties.TryGetProperty(out Duration duration) && _properties.TryGetProperty(out EaseType easeType))
            {
                var tween = CreateAnimatorObject("Tween").AddComponent<HandTweenAnimator>();
                tween.InitParams(duration, easeType);
                animators.Add(tween);
            }
            
            return animators.ToArray();
        }

        private void InitFingers()
        {
            foreach (var finger in _fingers)
                finger.Init();

            List<FingerBone> allBones = new List<FingerBone>();

            for (int i = 0; i < _fingers.Length; i++)
            {
                Finger finger = _fingers[i];

                for (int j = 0; j < finger.Bones.Length; j++)
                    allBones.Add(finger.Bones[j]);
            }

            _fingerBones = allBones.ToArray();
        }


    }
}