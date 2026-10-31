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
        [Tooltip("Seconds (random between x and y) from noticing something to reacting to it: the window to take a second enemy down.")]
        [SerializeField] private Vector2 _reactionTime;
        private float _fovUpdateTimer;
        private ITarget[] _potentialTargets;
        private Coroutine _reaction;
        public ITarget Target { get; private set; }

        public UnityEvent OnTargetFinded = new UnityEvent();
        /// <summary>An unaware enemy noticed something at this point and is about to react.</summary>
        public UnityEvent<Vector3> OnNoticed = new UnityEvent<Vector3>();

        private void Update()
        {
            if (!IsActive)
                return;

            if (Target != null || _reaction != null)
                return;

            if (_potentialTargets != null && _potentialTargets.Length == 0)
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

            Notice(targetsInView[0], targetsInView[0].TargetPoint.position);
        }

        public void ReciveSound(string name, float power, GameObject owner)
        {
            if (!IsActive)
                return;

            Notice(_potentialTargets[0], owner != null ? owner.transform.position : transform.position);
        }

        private void Notice(ITarget target, Vector3 source)
        {
            if (Target != null || _reaction != null)
                return;

            OnNoticed?.Invoke(source);

            float delay = Random.Range(_reactionTime.x, _reactionTime.y);
            if (delay > 0f && isActiveAndEnabled)
                _reaction = StartCoroutine(React(target, delay));
            else
                SetTarget(target);
        }

        private IEnumerator React(ITarget target, float delay)
        {
            yield return new WaitForSeconds(delay);
            _reaction = null;
            SetTarget(target);
        }

        private void SetTarget(ITarget target)
        {
            Target = target;
            OnTargetFinded?.Invoke();
        }
    }
}
