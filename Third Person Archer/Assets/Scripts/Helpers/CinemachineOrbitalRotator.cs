using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CinemachineOrbitalRotator : MonoBehaviour
{
    private CinemachineVirtualCamera VCamera;
    private CinemachineOrbitalTransposer _transposer;

    protected void Start()
    {
        VCamera = GetComponent<CinemachineVirtualCamera>();
        _transposer = VCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    private void Update()
    {
        if (VCamera.Priority > 0)
            _transposer.m_XAxis.m_InputAxisValue = 1;
        else
            _transposer.m_XAxis.m_InputAxisValue = 0;
    }
}
