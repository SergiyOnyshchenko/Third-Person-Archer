using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Actor
{
    public class CameraPOV : System
    {
        [SerializeField] private CinemachineVirtualCamera _tpvCamera;
        [SerializeField] private CinemachineVirtualCamera _fpvCamera;
        private const int _noPriorityIndex = 0;
        private const int _activePriorityIndex = 1;
        private CinemachineVirtualCamera[] _allCameras;

        private void Start()
        {
            _allCameras = new CinemachineVirtualCamera[] { _tpvCamera, _fpvCamera };
            SetThirdPerson();
        }

        public void SetThirdPerson()
        {
            SetCamera(_tpvCamera);
        }

        public void SetFirstPerson()
        {
            SetCamera(_fpvCamera);
        }

        private void SetCamera(CinemachineVirtualCamera camera)
        {
            ResetAllCameras();
            camera.Priority = _activePriorityIndex;
        }

        private void ResetAllCameras()
        {
            foreach (var camera in _allCameras)
                camera.Priority = _noPriorityIndex;
        }
    }
}
