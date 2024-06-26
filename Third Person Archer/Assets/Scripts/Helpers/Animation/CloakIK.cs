using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CloakIK : MonoBehaviour
{
    [SerializeField] private Transform _normalPoint;
    private SpringMotion _springMotion;

    private void Start()
    {
        _springMotion = new SpringMotion();
        _springMotion.SetTargetPosition(_normalPoint.position);
        _springMotion.SetTargetRotation(_normalPoint.rotation);
        _springMotion.SetSpringParams(2.5f, 0.25f);
    }

    private void FixedUpdate()
    {
        _springMotion.SetTargetPosition(_normalPoint.position);
        _springMotion.SetTargetRotation(_normalPoint.rotation);

        _springMotion.Update(transform);
    }
}
