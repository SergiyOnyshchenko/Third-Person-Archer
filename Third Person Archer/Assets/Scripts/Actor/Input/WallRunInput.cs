using UnityEngine;
using UnityEngine.Events;
using System;

namespace Actor
{
    public class WallRunInput : Input
    {
        public Transform[] RunPath { get; private set; }
        public bool IsRightDirection { get; private set; }

        public UnityEvent OnBeginWallRun = new UnityEvent();
        public UnityEvent OnFinishWallRun = new UnityEvent();

        private Action _onComplete;

        private void Start()
        {
            IsActive = true;
        }

        public void StartWallRun(Transform[] runPath, bool isRightDirection, Action onComplete = null)
        {
            if (!IsActive)
                return;

            RunPath = runPath;
            IsRightDirection = isRightDirection;
            _onComplete = onComplete;
            OnBeginWallRun?.Invoke();
        }

        public void FinishWallRun()
        {
            OnFinishWallRun?.Invoke();
            _onComplete?.Invoke();
            _onComplete = null;

            Debug.Log("FINISH INPUT");
        }
    }
}