using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class PerceptionInput : Input, ISoundListener
    {
        [SerializeField] private FieldOfView _fieldOfView;
        [SerializeField] private float _fovUpdateDelay = 0.5f;
        private float _fovUpdateTimer;
        private ITarget[] _potentialTargets;
        public ITarget Target { get; private set; }

        public UnityEvent OnTargetFinded = new UnityEvent();

        private void Update()
        {
            if (!IsActive)
                return;

            if (Target != null)
                return;

            _fovUpdateTimer += Time.deltaTime;

            if (_fovUpdateTimer >= _fovUpdateDelay)
            {
                UpdateFOV();
                _fovUpdateTimer = 0;
            }
        }

        public void ActivatePerception(ITarget[] potentialTargets)
        {
            _potentialTargets = potentialTargets;
            IsActive = true;
        }

        public void UpdateFOV()
        {
            ITarget[] targetsInView = _fieldOfView.GetTargetsInView(_potentialTargets);

            if (targetsInView == null || targetsInView.Length == 0)
                return;

            Target = targetsInView[0];
            OnTargetFinded?.Invoke();
        }

        public void ReciveSound(string name, float power, GameObject owner)
        {
            if (!IsActive)
                return;

            Target = _potentialTargets[0];
            OnTargetFinded?.Invoke();
        }
    }
}
