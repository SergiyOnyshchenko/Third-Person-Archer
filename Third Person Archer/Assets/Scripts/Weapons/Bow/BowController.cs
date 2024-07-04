using System.Collections;
using System.Collections.Generic;
using CustomAnimation;
using UnityEngine;
using CustomAnimation.Body;
using static RootMotion.FinalIK.AimPoser;
using DG.Tweening;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.EventSystems;
using Actor.Properties;

namespace Actor
{
    public class BowController : WeaponController, IActorIniter, IPull
    {
        [Header("Arrows")]
        [SerializeField] private GameObject _handArrow;
        [SerializeField] private GameObject _transitArrow;
        [Header("Animation")]
        [SerializeField] private BodyIKPoseData _hidePose;
        [SerializeField] private BodyIKPoseData _idlePose;
        [SerializeField] private BodyIKPoseData _pullPose;
        [SerializeField] private BodyIKPoseData _releasePose;
        [Space]
        [SerializeField] private Animation<BodyPartIK, BodyPartIKData> _reloadAnimation;
        private WeaponPull _weaponPull;
        private FpvController _fpv;
        private IBowView _bowView;
        private float _pullPower;
        private bool _isPulling;
        private GameObject _bowModel => _bowView.Model;
        private BowSpring _bowSpring => _bowView.BowSpring;
        private GameObject _bowArrow => _bowView.Arrow;
        public float PullPower { get => _pullPower; }
        public bool IsPulling { get => _isPulling;}
        public UnityEvent OnPullStarted = new UnityEvent();

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);

            if (actor.TryGetSystem(out BowViewController bowView))
                _bowView = bowView;

            if (actor.TryGetSystem(out FpvController fpv))
                _fpv = fpv;

            if (actor.TryGetProperty(out WeaponPull weaponPull))
                _weaponPull = weaponPull;

            if (actor.TryGetProperty(out BowAmmo ammo))
                _ammoCount = ammo;

            _reloadAnimation.Init(_fpv.FpvAnimator);
        }

        public void PlayAnimation(IAnimationPose<BodyPartIKData> pose)
        {
            _fpv.FpvAnimator.DoPose(pose);
        }

        #region Pulling

        public void SetStartSettings()
        {
            if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
                power.SetValue(12f);//power.SetValue(50f);

            _fpv.FpvAnimator.TrySetAnimator(AnimatorType.Spring);

            _bowModel.gameObject.SetActive(true);
            _fpv.ApplyHandsSpring(true);

            PlayAnimation(_idlePose);
        }

        public void BeginPull()
        {
            if (!CanAttack())
                return;

            SetPullPower(0);
            _bowSpring.SetHandIK(_fpv.RightHand.IkPoint);
            _bowArrow.SetActive(true);
            _isPulling = true;

            OnPullStarted?.Invoke();
        }

        public void HoldPull()
        {
            if (!CanAttack())
                return;

            SetPullPower(_pullPower + 1.1f * Time.fixedDeltaTime);

            var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
            PlayAnimation(lerpPose);
        }

        public void ReleasePull()
        {
            if (!CanAttack())
                return;

            Shoot(1f, () => SetTargetHitedEvent());

            _isPulling = false;
            _bowSpring.ResetHandIK();
            SetPullPower(0);
            _fpv.FpvAnimator.DoPose(_releasePose);
            _bowArrow.SetActive(false);
        }
        #endregion

        private void SetPullPower(float pull)
        {
            _pullPower = Mathf.Clamp(pull, 0f, 1f);
            _weaponPull.SetValue(_pullPower);
        }

        private void SetTargetHitedEvent()
        {
            
        }

        #region Reloading

        public void SetReloadSettings()
        {
            if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
                power.SetValue(5.5f);

            _handArrow.gameObject.SetActive(false);

            _transitArrow.transform.SetParent(_handArrow.transform.parent);
            _transitArrow.transform.localPosition = _handArrow.transform.localPosition;
            _transitArrow.transform.localRotation = _handArrow.transform.localRotation;
        }

        public void Reload(UnityAction onComplete)
        {
            float duration = 0.75f;

            DOVirtual.DelayedCall(0.5f, () =>
            {
                _reloadAnimation.Play(duration);
            });

            duration += 0.5f;

            DOVirtual.DelayedCall(duration / 4, () =>
            {
                _handArrow.transform.SetParent(_bowArrow.transform.parent);
                _handArrow.SetActive(true);
                _handArrow.transform.DOLocalMove(_bowArrow.transform.localPosition, 0.75f).SetUpdate(true);
                _handArrow.transform.DOLocalRotate(_bowArrow.transform.localEulerAngles, 0.75f).SetUpdate(true);
            });

            DOVirtual.DelayedCall(duration, () =>
            {
                _handArrow.SetActive(false);
                _bowArrow.SetActive(true);

                onComplete?.Invoke();
            });
        }

        #endregion
    }
}

