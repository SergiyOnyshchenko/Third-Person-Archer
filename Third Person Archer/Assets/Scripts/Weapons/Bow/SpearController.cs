using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using CustomAnimation;
using CustomAnimation.Body;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SpearController : WeaponController, IActorIniter
{
    [Header("Animation")]
    [SerializeField] private BodyIKPoseData _idlePose;
    [SerializeField] private BodyIKPoseData _pullPose;
    [SerializeField] private BodyIKPoseData _throwPose;
    [SerializeField] private BodyIKPoseData _reloadPose;
    private FpvController _fpv;
    private SpearHolder _spearHolder;
    private float _pullPower;
    public float PullPower { get => _pullPower; }


    public override void InitActor(ActorController actor)
    {
        base.InitActor(actor);

        if (actor.TryGetSystem(out FpvController fpv))
            _fpv = fpv;

        if (actor.TryGetProperty(out SpearAmmo ammo))
            _ammoCount = ammo;

        if (actor.TryGetPropertys(out SpearHolder[] holders))
            foreach (var holder in holders)
                if(holder.Pov == PovType.FirstPerson)
                    _spearHolder = holder;
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

        _spearHolder.ShowWeapon(true);
        _fpv.ApplyHandsSpring(true);

        PlayAnimation(_idlePose);
    }

    public void BeginPull()
    {
        if (!CanAttack())
            return;

        _pullPower = 0;
        _spearHolder.ShowWeapon(true);
    }

    public void HoldPull()
    {
        if (!CanAttack())
            return;

        _pullPower += 1.5f * Time.deltaTime;
        _pullPower = Mathf.Clamp(_pullPower, 0f, 1f);

        var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
        PlayAnimation(lerpPose);
    }

    public void ReleasePull()
    {
        if (!CanAttack())
            return;

        Shoot(_pullPower, () => SetTargetHitedEvent());

        _pullPower = 0;
        _spearHolder.ShowWeapon(false);

        _fpv.FpvAnimator.DoPose(_throwPose);
        DOVirtual.DelayedCall(0.15f, () => _fpv.FpvAnimator.DoPose(_reloadPose));
    }

    private void SetTargetHitedEvent()
    {

    }
}
