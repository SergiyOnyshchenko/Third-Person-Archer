using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringMotion : ISpringMotion
{
    private float _power;
    private float _dumping;
    private Vector3 _targetPosition;
    private Vector3 _positionVelocity = Vector3.zero;
    private Quaternion _targetRotation;
    private Quaternion _rotationVelocity = Quaternion.identity;
    private DampedSpringMotionParams _springParams;
    public void SetTargetPosition(Vector3 position) => _targetPosition = position;
    public void SetTargetRotation(Quaternion rotation) => _targetRotation = rotation;
    public void UpdateMotionParams() => 
        SpringCalculator.CalcDampedSpringMotionParams(ref _springParams, Time.fixedUnscaledDeltaTime, _power, _dumping);

    public SpringMotion(){}

    public SpringMotion(float power, float dumping) 
    {
        SetSpringParams(power, dumping);
    }

    public void SetSpringParams(float power, float dumping)
    {
        if(_power == power && _dumping == dumping)
            return;

        _power = power;
        _dumping = dumping;

        UpdateMotionParams();
    }

    public void Update(Transform target)
    {
        UpdatePosition(target);
        UpdateRotation(target); 
    }

    private void UpdatePosition(Transform target)
    {
        Vector3 position = target.localPosition;
        SpringCalculator.UpdateDampedSpringMotion(ref position, ref _positionVelocity, _targetPosition, _springParams);
        target.localPosition = position;
    }

    private void UpdateRotation(Transform target)
    {
        Quaternion rotation = target.localRotation;
        SpringCalculator.UpdateDampedSpringMotion(ref rotation, ref _rotationVelocity, _targetRotation, _springParams);
        target.localRotation = rotation;
    }
}
