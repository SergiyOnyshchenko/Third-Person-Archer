using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPositioner : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void Update()
    {
        if(_target != null)
            transform.position = _target.position;
    }
}
