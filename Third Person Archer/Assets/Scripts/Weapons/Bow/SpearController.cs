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

public class SpearController : WeaponController, IActorIniter, IPull
{
    [Header("Animation")]
    [SerializeField] private BodyIKPoseData _idlePose;
    [SerializeField] private BodyIKPoseData _pullPose;
    [SerializeField] private BodyIKPoseData _throwPose;
    [SerializeField] private BodyIKPoseData _reloadPose;
    private FpvController _fpv;
    private SpearHolder _spearHolder;
    private WeaponPull _weaponPull;
    private bool _isPulling;
    private float _pullPower;
    public float PullPower { get => _pullPower; }
    public bool IsPulling { get => _isPulling; }

    public override void InitActor(ActorController actor)
    {
        base.InitActor(actor);

        if (actor.TryGetSystem(out FpvController fpv))
            _fpv = fpv;

        if (actor.TryGetProperty(out SpearAmmo ammo))
            _ammoCount = ammo;

        if (actor.TryGetProperty(out WeaponPull weaponPull))
            _weaponPull = weaponPull;

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

        _isPulling = true;
        SetPullPower(0);
        _spearHolder.ShowWeapon(true);
    }

    public void HoldPull()
    {
        if (!CanAttack())
            return;

        SetPullPower(_pullPower + 1f * Time.fixedDeltaTime);

        var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
        PlayAnimation(lerpPose);
    }

    public void ReleasePull()
    {
        if (!CanAttack())
            return;

        Shoot(1f, () => SetTargetHitedEvent());

        _isPulling = false;
        SetPullPower(0);
        _spearHolder.ShowWeapon(false);

        _fpv.FpvAnimator.DoPose(_throwPose);
        DOVirtual.DelayedCall(0.15f, () => _fpv.FpvAnimator.DoPose(_reloadPose));
    }

    private void SetPullPower(float pull)
    {
        _pullPower = Mathf.Clamp(pull, 0f, 1f);
        _weaponPull.SetValue(_pullPower);
    }

    private void SetTargetHitedEvent()
    {

    }
}
