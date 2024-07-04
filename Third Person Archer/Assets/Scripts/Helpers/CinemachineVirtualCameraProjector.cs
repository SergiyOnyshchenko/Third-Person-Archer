using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineVirtualCameraProjector : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;

    private void Update()
    {
        transform.position = _camera.State.CorrectedPosition;
        transform.rotation = _camera.State.CorrectedOrientation;
    }
}
