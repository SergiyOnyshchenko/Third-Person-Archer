using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    [SerializeField] private float _speed = 100;
    [SerializeField] private Vector3 _axis = Vector3.up;
    private bool _isActive = true;

    private void Update()
    {
        if(_isActive)
            transform.Rotate(_axis * _speed * Time.deltaTime);
    }

    public void Stop()
    {
        _isActive = false;
    }
}
