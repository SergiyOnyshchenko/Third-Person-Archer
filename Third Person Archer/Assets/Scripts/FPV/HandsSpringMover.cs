using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsSpringMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _springFrequency = 3f;
    [SerializeField] private float _springDamping = 0.2f;
    [SerializeField] private WeaponLookRotator _rotator;
    private DampedSpringMotionParams _springMotionParams;
    private Vector3 _velocity = Vector3.zero;
    private float xVelocity;
    private float yVelocity;
    private float zVelocity;
    private float wVelocity;

    private void Start() 
    {
        //_target = Camera.main.transform;
        SpringCalculator.CalcDampedSpringMotionParams(ref _springMotionParams, Time.fixedDeltaTime, _springFrequency * 1.5f, _springDamping);
    }

    private void FixedUpdate() 
    {
        Quaternion rotation = transform.rotation;
        float x = rotation.x;
        float y = rotation.y;
        float z = rotation.z;
        float w = rotation.w;

        SpringCalculator.UpdateDampedSpringMotion(ref x, ref xVelocity, _target.rotation.x, _springMotionParams);
        SpringCalculator.UpdateDampedSpringMotion(ref y, ref yVelocity, _target.rotation.y, _springMotionParams);
        SpringCalculator.UpdateDampedSpringMotion(ref z, ref zVelocity, _target.rotation.z, _springMotionParams);
        SpringCalculator.UpdateDampedSpringMotion(ref w, ref wVelocity, _target.rotation.w, _springMotionParams);

        transform.rotation = new Quaternion(x, y, z, w);
    }
}