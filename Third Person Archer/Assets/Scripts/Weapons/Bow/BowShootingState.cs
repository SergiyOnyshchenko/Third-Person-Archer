using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.FPV;
using CustomAnimation.Body;
using DG.Tweening;

public class BowShootingState : ProcessState
{
    [SerializeField] private Projectile _projectile;
    [SerializeField] private Transform _directionCamera;
    [Space]
    [SerializeField] private FpvAnimatorController _animatorController;
    [Space]
    [SerializeField] private BodyIKPoseData _hidePose;
    [SerializeField] private BodyIKPoseData _idlePose;
    [SerializeField] private BodyIKPoseData _pullPose;
    [SerializeField] private BodyIKPoseData _releasePose;
    [Space]
    [SerializeField] private BowSpring _bowSpring;
    [SerializeField] private GameObject _bowArrow;
    [Space]
    [SerializeField] private BodyPartIK _leftHand;
    [SerializeField] private BodyPartIK _rightHand;
    private float _pullPower;
    public float PullPower { get => _pullPower; }

    public override void Enter()
    {
        base.Enter();

        _pullPower = 0;

        //_leftHand.UseSpring = true;
        //_rightHand.UseSpring = true;

        if (_animatorController.Properties.TryGetProperty(out SpringPower power))
        {
            power.SetValue(2.5f);
        }

        _animatorController.TrySetAnimator(AnimatorType.Spring);
        _animatorController.DoPose(_idlePose);
    }

    public override void Exit() 
    {
        //_leftHand.UseSpring = false;
        //_rightHand.UseSpring = false;

        base.Exit();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _pullPower = 0;
            _bowSpring.SetHandIK(_rightHand.IkPoint);

            _bowArrow.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            _pullPower += 0.75f * Time.deltaTime;
            _pullPower = Mathf.Clamp(_pullPower, 0f, 1f);

            var lerpPose = _animatorController.LerpPoses(_idlePose, _pullPose, _pullPower);
            _animatorController.DoPose(lerpPose);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ShootProjectile();

            _bowSpring.ResetHandIK();
            _pullPower = 0;
            _animatorController.DoPose(_releasePose);

            _bowArrow.SetActive(false);

            DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }

    private void ShootProjectile()
    {
        Projectile arrow = Instantiate(_projectile, _bowArrow.transform.position, _bowArrow.transform.rotation);
        arrow.Shoot(_directionCamera.forward, _pullPower);
    }
}
