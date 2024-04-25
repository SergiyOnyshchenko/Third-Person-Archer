using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowSpring : MonoBehaviour
{
    [SerializeField] private Transform _handIK;
    [SerializeField] private Transform _holdPoint;
    [Space]
    [SerializeField] private float _springPower = 10;
    [SerializeField] private float _springDumping = 1;
    private SpringFloat _spring;
    private Vector3 _normalPosition;

    private void Awake()
    {
        _normalPosition = _holdPoint.transform.localPosition;
        _spring = new SpringFloat(_springPower, _springDumping, _normalPosition.y);
    }

    private void FixedUpdate()
    {
        if (_handIK == null)
        {
            _spring.UpdateValue(_normalPosition.y);

            Vector3 localPosition = _holdPoint.localPosition;
            localPosition.y = _spring.Value;
            _holdPoint.localPosition = localPosition;
        }
        else
        {
            Vector3 handPosition = _handIK.position;

            handPosition = _holdPoint.parent.InverseTransformPoint(handPosition);
            handPosition.x = 0;
            handPosition.z = 0;

            _holdPoint.localPosition = handPosition;
        }
    }

    public void SetHandIK(Transform target)
    {
        _handIK = target;
    }

    public void ResetHandIK()
    {
        _handIK = null;
        _spring = new SpringFloat(_springPower, _springDumping);
    }
}
