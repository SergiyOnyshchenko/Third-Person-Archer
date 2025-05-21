using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayCameraHelper : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _overlayCamera;

    private void Update()
    {
        _overlayCamera.fieldOfView = _mainCamera.fieldOfView;
    }
}
