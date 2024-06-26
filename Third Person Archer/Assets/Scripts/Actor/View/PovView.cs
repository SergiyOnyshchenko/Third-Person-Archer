using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Actor
{
    public class PovView : MonoBehaviour
    {
        [SerializeField] private PovType _povType;
        [SerializeField] private CinemachineVirtualCamera _camera;
    }
}