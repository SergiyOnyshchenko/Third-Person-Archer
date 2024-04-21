using System.Collections;
using System.Collections.Generic;
using CustomAnimation.Body;
using RootMotion.FinalIK;
using UnityEngine;
using static RootMotion.FinalIK.AimPoser;

namespace CustomAnimation.FPV
{
    public class FpvAnimatorController : AnimatorController<BodyPartIK, BodyPartIKData>
    {
        [Space]
        [SerializeField] private BodyIKPoseData _startPose;
        [Space]
        [SerializeField] private FpvIK _body;
        public FpvIK Body { get => _body; }

        private void Start()
        {
            if (_startPose != null)
                DoPose(_startPose);
        }

        public override void DoPose(IAnimationPose<BodyPartIKData> pose)
        {
            BodyPartIK[] bodyParts = _body.GetBodyParts();
            BodyPartIKData[] poseData = pose.Instances;

            for (int i = 0; i < bodyParts.Length; i++)
                _currentAnimator.DoPose(bodyParts[i], poseData[i]);
        }

        public override IAnimationPose<BodyPartIKData> LerpPoses(
            IAnimationPose<BodyPartIKData> pose1,
            IAnimationPose<BodyPartIKData> pose2,
            float value)
        {
            var newPose = new AnimationPose<BodyPartIKData>();
            newPose.Instances = new BodyPartIKData[pose1.Instances.Length];

            for (int i = 0; i < newPose.Instances.Length; i++)
            {
                var bodyPart1 = pose1.Instances[i];
                var bodyPart2 = pose2.Instances[i];

                Vector3 position = Vector3.Lerp(bodyPart1.Position, bodyPart2.Position, value);
                Quaternion rotation = Quaternion.Lerp(bodyPart1.Rotation, bodyPart2.Rotation, value);

                newPose.Instances[i] = new BodyPartIKData(bodyPart1.Name, position, rotation);
            }

            return newPose;
        }

        public override BodyPartIKData[] GetPoseData() => _body.GetBodyPartsData();

        protected override AnimationProperty[] InitProperties()
        {
            List<AnimationProperty> properties = new List<AnimationProperty>();

            var duration = CreatePropertyObject("Duration").AddComponent<Duration>();
            duration.Init(0.5f);
            properties.Add(duration);

            var easeType = CreatePropertyObject("Ease").AddComponent<EaseType>();
            easeType.Init(DG.Tweening.Ease.Linear);
            properties.Add(easeType);

            var springPower = CreatePropertyObject("SpringPower").AddComponent<SpringPower>();
            //springPower.Init(2.25f);
            springPower.Init(2.5f);
            properties.Add(springPower);

            var springDumpingr = CreatePropertyObject("SpringDumping").AddComponent<SpringDumping>();
            //springDumpingr.Init(0.65f);
            springDumpingr.Init(0.5f);
            properties.Add(springDumpingr);

            return properties.ToArray();
        }

        protected override Animator<BodyPartIK, BodyPartIKData>[] InitAnimators()
        {
            List<Animator<BodyPartIK, BodyPartIKData>> animators = new List<Animator<BodyPartIK, BodyPartIKData>>();

            var instant = CreateAnimatorObject("Instant").AddComponent<BodyInstantAnimator>();
            animators.Add(instant);

            if (_properties.TryGetProperty(out Duration duration) && _properties.TryGetProperty(out EaseType easeType))
            {
                var tween = CreateAnimatorObject("Tween").AddComponent<BodyTweenAnimator>();
                tween.InitParams(duration, easeType);
                animators.Add(tween);
            }

            if (_properties.TryGetProperty(out SpringPower springPower) && _properties.TryGetProperty(out SpringDumping springDumping))
            {
                var spring = CreateAnimatorObject("Spring").AddComponent<BodySpringAnimator>();
                spring.InitParams(springPower, springDumping);
                animators.Add(spring);
            }

            return animators.ToArray();
        }
    }
}

