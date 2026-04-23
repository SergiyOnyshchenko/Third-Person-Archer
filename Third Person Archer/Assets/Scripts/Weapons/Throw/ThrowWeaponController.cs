using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;
using CustomAnimation;
using CustomAnimation.Body;
using DG.Tweening;
using UI.HUD;

public abstract class ThrowWeaponController : WeaponController, IActorIniter, IPull
{
    [Header("Animation")]
    [SerializeField] private BodyIKPoseData _idlePose;
    [SerializeField] private BodyIKPoseData _pullPose;
    [SerializeField] private BodyIKPoseData _throwPose;
    [SerializeField] private BodyIKPoseData _reloadPose;
    [Header("States")]
    [SerializeField] private DelayTransition _reloadTransiiton;
    private FpvController _fpv;
    protected WeaponHolder _weaponHolder;
    private WeaponPull _weaponPull;
    private float _reloadDuration = 1f;
    private bool _isPulling;
    private float _pullPower;
    public float PullPower { get => _pullPower; }
    public bool IsPulling { get => _isPulling; }

    protected abstract void InitWeaponHolders(ActorController actor);

    public override void InitActor(ActorController actor)
    {
        base.InitActor(actor);

        if (actor.TryGetSystem(out FpvController fpv))
            _fpv = fpv;

        if (actor.TryGetProperty(out WeaponPull weaponPull))
            _weaponPull = weaponPull;

        InitWeaponHolders(actor);
    }

    public void PlayAnimation(IAnimationPose<BodyPartIKData> pose)
    {
        _fpv.FpvAnimator.DoPose(pose);
    }

    #region Settings
    public void SetStartSettings()
    {
        if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
            power.SetValue(18f);

        _fpv.FpvAnimator.TrySetAnimator(AnimatorType.Spring);

        _weaponHolder.ShowWeapon(true);
        _fpv.ApplyHandsSpring(true);

        PlayAnimation(_idlePose);
    }

    public void SetReloadDuration(float duration)
    {
        _reloadTransiiton.SetDelay(duration);
        _reloadDuration = duration;
    }
    #endregion

    #region Pulling
    public void BeginPull()
    {
        if (!CanAttack())
            return;

        _isPulling = true;
        SetPullPower(0);
        _weaponHolder.ShowWeapon(true);
    }

    public void HoldPull()
    {
        if (!CanAttack())
            return;

        SetPullPower(_pullPower + 5f * Time.fixedDeltaTime);
        UpdatePose();
    }

    public void UnHoldPull()
    {
        if (!CanAttack())
            return;

        SetPullPower(_pullPower - 5f * Time.fixedDeltaTime);
        UpdatePose();
    }

    public void ReleasePull()
    {
        if (!CanAttack())
            return;

        Shoot(1f, () => SetTargetHitedEvent());

        _isPulling = false;
        SetPullPower(0);
        _weaponHolder.ShowWeapon(false);

        _fpv.FpvAnimator.DoPose(_throwPose);
        DOVirtual.DelayedCall(0.15f, () => _fpv.FpvAnimator.DoPose(_reloadPose));

        if (_reloadDuration > 0f)
        {
            ReloadSignals.Start(_reloadDuration);
            DG.Tweening.DOVirtual.DelayedCall(_reloadDuration, ReloadSignals.End);
        }
    }

    public void UpdatePose()
    {
        var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
        PlayAnimation(lerpPose);
    }

    private void SetPullPower(float pull)
    {
        _pullPower = Mathf.Clamp(pull, 0f, 1f);
        _weaponPull.SetValue(_pullPower);
    }
    #endregion

    private void SetTargetHitedEvent() {}
}
