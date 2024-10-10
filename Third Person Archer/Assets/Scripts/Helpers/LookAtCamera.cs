using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _camera;

    private void Awake()
    {
        _camera = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(_camera.position);
    }
}
