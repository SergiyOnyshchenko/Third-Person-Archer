using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using DG.Tweening;

namespace Actor
{
    public class CameraPOV : System
    {
        [SerializeField] private CinemachineVirtualCamera _tpvCamera;
        [SerializeField] private GameObject _tpvModel;
        [Space]
        [SerializeField] private CinemachineVirtualCamera _fpvCamera;
        [SerializeField] private GameObject _fpvModel;
        [SerializeField] private Transform _fpvProjector;
        private const int _noPriorityIndex = 0;
        private const int _activePriorityIndex = 1;
        private CinemachineVirtualCamera[] _allCameras;
        private GameObject[] _allModels;
        private CinemachineBrain _cinemachineBrain;
        public CinemachineVirtualCamera FpvCamera => _fpvCamera;
        public Transform FpvProjector => _fpvProjector;
        public UnityEvent OnFPV = new UnityEvent();
        public UnityEvent OnFTV = new UnityEvent();
   
        private void Start()
        {
            _cinemachineBrain = FindObjectOfType<CinemachineBrain>();

            _allCameras = new CinemachineVirtualCamera[] { _tpvCamera, _fpvCamera};
            _allModels = new GameObject[] { _tpvModel, _fpvModel };

            SetThirdPerson();
        }

        public void SetThirdPerson()
        {
            SetCamera(_tpvCamera);

            DOVirtual.DelayedCall(0.1f, () => 
            {
                SetModel(_tpvModel);
                OnFTV?.Invoke();
            });
        }

        public void SetFirstPerson()
        {
            _fpvCamera.gameObject.SetActive(false);
            _fpvCamera.gameObject.SetActive(true);

            SetCamera(_fpvCamera);

            DOVirtual.DelayedCall(0.9f, () => 
            {
                SetModel(_fpvModel);
                OnFPV?.Invoke();
            });
        }

        private void SetCamera(CinemachineVirtualCamera camera)
        {
            ResetAllCameras();
            camera.Priority = _activePriorityIndex;
        }

        private void SetModel(GameObject model)
        {
            ResetAllModels();
            model.SetActive(true);
        }

        private void ResetAllCameras()
        {
            foreach (var camera in _allCameras)
                camera.Priority = _noPriorityIndex;
        }

        private void ResetAllModels()
        {
            foreach (var model in _allModels)
                model.SetActive(false);
        }
    }
}
