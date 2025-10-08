using System.Collections;
using System.Collections.Generic;
using CustomAnimation;
using UnityEngine;
using CustomAnimation.Body;
using DG.Tweening;
using UnityEngine.Events;
using Game.Weapons;
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
        private float _reloadDuration = 1f;
        private GameObject _bowModel => _bowView.Model;
        private BowSpring _bowSpring => _bowView.BowSpring;
        private GameObject _bowArrow => _bowView.Arrow;
        public float PullPower => _weaponPull.Value;
        public bool IsPulling => _weaponPull.Value > 0;

        public UnityEvent OnPullStarted = new UnityEvent();

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);

            if (actor.TryGetSystem(out BowFpvSkinView bowView))
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
                power.SetValue(12f);

            _fpv.FpvAnimator.TrySetAnimator(AnimatorType.Spring);

            _bowModel.gameObject.SetActive(true);
            _fpv.ApplyHandsSpring(true);

            PlayAnimation(_idlePose);
        }

        public void BeginPull()
        {
            _bowSpring.SetHandIK(_fpv.RightHand.IkPoint);
            _bowArrow.SetActive(true);

            OnPullStarted?.Invoke();
        }

        public void HoldPull()
        {
            var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _weaponPull.Value);
            PlayAnimation(lerpPose);
        }

        public void ReleasePull()
        {
            Shoot(1f, () => SetTargetHitedEvent());

            _bowSpring.ResetHandIK();
            _fpv.FpvAnimator.DoPose(_releasePose);
            _bowArrow.SetActive(false);
        }
        #endregion

        private void SetTargetHitedEvent() {}

        #region Reloading

        public void SetReloadSettings()
        {
            if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
                power.SetValue(10f);

            _handArrow.gameObject.SetActive(false);
            _transitArrow.gameObject.SetActive(true);

            _transitArrow.transform.SetParent(_handArrow.transform.parent);
            _transitArrow.transform.localPosition = _handArrow.transform.localPosition;
            _transitArrow.transform.localRotation = _handArrow.transform.localRotation;
        }

        public void SetReloadDuration(float value)
        {
            _reloadDuration = value;
        }

        public void Reload(UnityAction onComplete)
        {
            float duration = _reloadDuration;
            UI.HUD.ReloadSignals.Start(duration);
            
            DOVirtual.DelayedCall(0, () =>
            {
                _bowArrow.SetActive(false);
                _reloadAnimation.Play(duration, false, () =>
                {
                    _handArrow.SetActive(false);
                    _transitArrow.SetActive(false);
                    _bowArrow.SetActive(true);

                    UI.HUD.ReloadSignals.End();

                    onComplete?.Invoke();
                });
            });

            DOVirtual.DelayedCall(duration - duration / 8, () =>
            {
                _transitArrow.transform.SetParent(_bowArrow.transform.parent);
                _transitArrow.transform.DOLocalMove(_bowArrow.transform.localPosition, duration/8).SetUpdate(true);
                _transitArrow.transform.DOLocalRotate(_bowArrow.transform.localPosition, duration/8).SetUpdate(true);
            });

        }

        public void ResetReloadSettings()
        {
            _handArrow.SetActive(false);
            _transitArrow.gameObject.SetActive(false);
        }

        #endregion
    }
}

