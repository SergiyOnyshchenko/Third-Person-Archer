using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleIKBinder : MonoBehaviour
{
    [SerializeField] private Transform _targetIK;
    [SerializeField] private Transform _myIK;

    private void Awake()
    {
        if(_myIK == null)
            _myIK = transform;
    }

    private void Update()
    {
        if (_targetIK == null)
            return;

        _myIK.transform.position = _targetIK.transform.position;
    }

    public void SetTargetIK(Transform target)
    {
        _targetIK = target;
    }
}
