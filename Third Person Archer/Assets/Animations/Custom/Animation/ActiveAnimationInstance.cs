using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.Body;
using CustomAnimation.Hand;

[System.Serializable]
public class ActiveAnimationInstance<T, D> : IActiveAnimation where T : class where D : class
{
    [SerializeField] private AnimationPoseValue<D>[] _poses;
    [Space]
    [SerializeField] private AnimatorController<T, D> _animatorController;
    [SerializeField] private AnimatorType _targetAnimator;
    [SerializeField] private AnimationProperty[] _propertiesOverride;
    public AnimatorController<T, D> AnimatorController { get => _animatorController; }
    public AnimationProperty[] PropertiesOverride { get => _propertiesOverride; }
    public AnimatorType AnimatorType { get => _targetAnimator; }
    public AnimationPoseValue<D>[] Poses { get => _poses; }
    public bool IsInited => _animatorController != null && _poses != null;

    public void Play()
    {
        if (!IsInited)
            return;

        _animatorController.TrySetAnimator(_targetAnimator);
    }

    public void Stop()
    {

    }

    public void SetAnimationValue(float value)
    {
        if (!IsInited)
            return;

        var newPose = LerpPoses(_poses, value);
        _animatorController.DoPose(newPose);
    }

    private IAnimationPose<D> LerpPoses(AnimationPoseValue<D>[] poses, float value)
    {
        var previousPose = poses[0];
        var nextPose = poses[0];

        for (int i = 0; i < poses.Length; i++)
        {
            nextPose = poses[i];

            if (value <= nextPose.Value)
                break;

            previousPose = poses[i];
        }

        value = Mathf.InverseLerp(previousPose.Value, nextPose.Value, value);

        return _animatorController.LerpPoses(previousPose.Pose, nextPose.Pose, value);
    }
}

[System.Serializable]
public class ActiveBodyIKAnimation : ActiveAnimationInstance<BodyPartIK, BodyPartIKData> { }

[System.Serializable]
public class ActiveHandAnimation : ActiveAnimationInstance<FingerBone, FingerBoneData> { }