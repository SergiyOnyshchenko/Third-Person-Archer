using System.Collections;
using System.Collections.Generic;
using CustomAnimation;
using UnityEngine;
using CustomAnimation.Body;
using static RootMotion.FinalIK.AimPoser;
using DG.Tweening;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class BowController : WeaponController, IActorIniter
    {
        [Header("Bow")]
        [SerializeField] private GameObject _bowModel;
        [SerializeField] private BowSpring _bowSpring;
        [Header("Arrow")]
        [SerializeField] private GameObject _bowArrow;
        [SerializeField] private GameObject _handArrow;
        [SerializeField] private GameObject _transitArrow;
        [Header("Animation")]
        [SerializeField] private BodyIKPoseData _hidePose;
        [SerializeField] private BodyIKPoseData _idlePose;
        [SerializeField] private BodyIKPoseData _pullPose;
        [SerializeField] private BodyIKPoseData _releasePose;
        [Space]
        [SerializeField] private Animation<BodyPartIK, BodyPartIKData> _reloadAnimation;
        private FpvController _fpv;
        private float _pullPower;
        public float PullPower { get => _pullPower; }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out FpvController fpv))
                _fpv = fpv;

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
                power.SetValue(3.5f);

            _fpv.FpvAnimator.TrySetAnimator(AnimatorType.Spring);

            _bowModel.gameObject.SetActive(true);
            _fpv.ApplyHandsSpring(true);
        }

        public void BeginPull()
        {
            _pullPower = 0;
            _bowSpring.SetHandIK(_fpv.RightHand.IkPoint);
            _bowArrow.SetActive(true);
        }

        public void HoldPull()
        {
            _pullPower += 0.75f * Time.deltaTime;
            _pullPower = Mathf.Clamp(_pullPower, 0f, 1f);

            var lerpPose = _fpv.FpvAnimator.LerpPoses(_idlePose, _pullPose, _pullPower);
            PlayAnimation(lerpPose);
        }

        public void ReleasePull()
        {
            _bowSpring.ResetHandIK();
            _pullPower = 0;
            _fpv.FpvAnimator.DoPose(_releasePose);
            _bowArrow.SetActive(false);
        }
        #endregion

        #region Reloading

        public void SetReloadSettings()
        {
            if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
                power.SetValue(5f);

            _handArrow.gameObject.SetActive(false);

            _transitArrow.transform.SetParent(_handArrow.transform.parent);
            _transitArrow.transform.localPosition = _handArrow.transform.localPosition;
            _transitArrow.transform.localRotation = _handArrow.transform.localRotation;
        }

        public void Reload(UnityAction onComplete)
        {
            float duration = 1;

            _reloadAnimation.Play(duration);

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

