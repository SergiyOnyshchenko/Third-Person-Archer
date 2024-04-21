using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.FPV;
using CustomAnimation.Body;
using DG.Tweening;

public class BowReloadingState : ProcessState
{
    [SerializeField] private FpvAnimatorController _animatorController;
    [Space]
    [SerializeField] private BodyIKPoseData[] _poses;
    [Space]
    [SerializeField] private GameObject _bowArrow;
    [SerializeField] private GameObject _handArrowMain;
    [SerializeField] private GameObject _handArrow;
    private float _value = 0;
    private int _index = 0;

    public override void Enter()
    {
        base.Enter();
        _value = 0;
        _index = 0;

        _handArrowMain.gameObject.SetActive(false);

        _handArrow.transform.SetParent(_handArrowMain.transform.parent);
        _handArrow.transform.localPosition = _handArrowMain.transform.localPosition;
        _handArrow.transform.localRotation = _handArrowMain.transform.localRotation;

        //_animatorController.TrySetAnimator(AnimatorType.Instant);

        if (_animatorController.Properties.TryGetProperty(out SpringPower power))
        {
            power.SetValue(1.25f);
        }
    }

    private void Update()
    {
        _value += 2.5f * Time.deltaTime;

        if ((float)_index + 1 < _value)
        {
            _index++;

            if(_index == 2)
            {
                _handArrow.transform.SetParent(_bowArrow.transform.parent);
                _handArrow.SetActive(true);

                _handArrow.transform.DOLocalMove(_bowArrow.transform.localPosition, 0.75f);
                _handArrow.transform.DOLocalRotate(_bowArrow.transform.localEulerAngles, 0.75f);
            }

            if (_index == 4)
            {
     
            }
        }
            

        if (_index == _poses.Length - 1)
        {
            _handArrow.SetActive(false);
            _bowArrow.SetActive(true);

            FinishProcess();
            return;
        }

        var pose = _animatorController.LerpPoses(_poses[_index], _poses[_index + 1], _value - (float)_index);
        _animatorController.DoPose(pose);
    }

    
}
