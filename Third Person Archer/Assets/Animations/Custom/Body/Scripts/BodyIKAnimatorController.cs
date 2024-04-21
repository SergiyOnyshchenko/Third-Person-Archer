using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEditor.ShaderGraph;
using JetBrains.Annotations;

namespace CustomAnimation.Body
{
    public class BodyIKAnimatorController : AnimatorController<BodyPartIK, BodyPartIKData>
    {
        [Space]
        [SerializeField] private BodyIK _body;
        [Space]
        [SerializeField] private BipedIK _bipedIK;
        public BodyIK Body { get => _body; }

        public override void DoPose(IAnimationPose<BodyPartIKData> pose)
        {
            BodyPartIK[] bodyParts = _body.GetBodyParts();
            BodyPartIKData[] poseData = pose.Instances;

            //for (int i = 0; i < bodyParts.Length; i++)
            //    _currentAnimator.DoPose(bodyParts[i], poseData[i]);

            foreach (var bodyPart in bodyParts)
            {
                foreach (var data in poseData)
                {
                    if (bodyPart.Name == data.Name)
                    {
                        _currentAnimator.DoPose(bodyPart, data);
                        break;
                    }
                }
            }
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
            duration.Init(1f);
            properties.Add(duration);

            var easeType = CreatePropertyObject("Ease").AddComponent<EaseType>();
            easeType.Init(DG.Tweening.Ease.Linear);
            properties.Add(easeType);

            var springPower = CreatePropertyObject("SpringPower").AddComponent<SpringPower>();
            springPower.Init(4f);
            properties.Add(springPower);

            var springDumpingr = CreatePropertyObject("SpringDumping").AddComponent<SpringDumping>();
            springDumpingr.Init(0.25f);
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
