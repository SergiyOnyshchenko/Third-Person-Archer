using System.Collections;
using System.Collections.Generic;
using Actor;
using CustomAnimation;
using CustomAnimation.Body;
using DG.Tweening;
using UnityEngine;

public class SpearController : WeaponController, IActorIniter
{
    [Header("View")]
    [SerializeField] private GameObject _spearModel;
    [Header("Animation")]
    [SerializeField] private BodyIKPoseData _idlePose;
    [SerializeField] private BodyIKPoseData _pullPose;
    [SerializeField] private BodyIKPoseData _throwPose;
    [SerializeField] private BodyIKPoseData _reloadPose;
    private FpvController _fpv;
    private float _pullPower;
    public float PullPower { get => _pullPower; }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out FpvController fpv))
            _fpv = fpv;
    }

    public void PlayAnimation(IAnimationPose<BodyPartIKData> pose)
    {
        _fpv.FpvAnimator.DoPose(pose);
    }

    public void SetStartSettings()
    {
        if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
            power.SetValue(8f);

        _fpv.FpvAnimator.TrySetAnimator(AnimatorType.Spring);

        _spearModel.SetActive(true);
        _fpv.ApplyHandsSpring(true);

        PlayAnimation(_idlePose);
    }

    public void BeginPull()
    {
        _pullPower = 0;
        _spearModel.SetActive(true);
    }

    public void HoldPull()
    {
        _pullPower += 1.5f * Time.deltaTime;
        _pullPower = Mathf.Clamp(_pullPower, 0f, 1f);

        var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
        PlayAnimation(lerpPose);
    }

    public void ReleasePull()
    {
        _pullPower = 0;
        _spearModel.SetActive(false);

        _fpv.FpvAnimator.DoPose(_throwPose);
        DOVirtual.DelayedCall(0.15f, () => _fpv.FpvAnimator.DoPose(_reloadPose));
    }
}
