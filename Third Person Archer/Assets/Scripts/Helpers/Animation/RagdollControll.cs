using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class RagdollControll : System
    {
        [SerializeField] private UnityEngine.Animator _animator;
        [SerializeField] private Rigidbody[] _allRigidbodys;
        [SerializeField] private bool _isPhysical;

        [Header("Hit impulse")]
        [Tooltip("Velocity change of the bone the projectile hit, m/s.")]
        [SerializeField] private float _hitBoneSpeed = 9f;
        [Tooltip("Velocity change of the whole body along the hit, m/s.")]
        [SerializeField] private float _bodySpeed = 2.5f;
        [Tooltip("Multiplier for hits on critical (head) hitboxes.")]
        [SerializeField] private float _criticalMultiplier = 1.6f;
        [Tooltip("A hit counts for the death impulse if the body goes physical within this many real seconds.")]
        [SerializeField] private float _pushLifetime = 0.5f;

        private struct PendingPush
        {
            public Rigidbody Body;
            public Vector3 Direction;
            public float Strength;
            public float Time;
        }

        private readonly List<PendingPush> _pendingPushes = new List<PendingPush>();
        private bool _settling;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<UnityEngine.Animator>();

            //_allRigidbodys = GetComponentsInChildren<Rigidbody>();

            MakeKinematic();
        }

        public void MakeKinematic()
        {
            if (_animator != null)
                _animator.enabled = true;

            foreach (Rigidbody rigidbody in _allRigidbodys)
                rigidbody.isKinematic = true;

            _isPhysical = false;
        }

        public void MakePhysical()
        {
            if (_isPhysical)
                return;

            if (_animator != null)
                _animator.enabled = false;

            _isPhysical = true;

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Delay());
                StartCoroutine(Freeze());
            }
            else
            {
                foreach (Rigidbody rigidbody in _allRigidbodys)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.linearVelocity = Vector3.zero;
                }
                ApplyPendingPushes();
            }
        }

        /// <summary>
        /// Knocks the body along a projectile hit. The push waits until the ragdoll has gone physical and settled,
        /// so a killing hit always throws the body, however the death state and the frame rate line up.
        /// </summary>
        public void Push(Rigidbody hitBone, Vector3 direction, bool critical)
        {
            var push = new PendingPush
            {
                Body = hitBone,
                Direction = direction.normalized,
                Strength = critical ? _criticalMultiplier : 1f,
                Time = Time.unscaledTime,
            };

            if (_isPhysical && !_settling)
                Apply(push);
            else
                _pendingPushes.Add(push);
        }

        private IEnumerator Delay()
        {
            // Bodies switched from animation can carry huge velocities; zero them for a few frames, then push.
            _settling = true;
            foreach (Rigidbody rigidbody in _allRigidbodys)
            {
                rigidbody.isKinematic = false;
                rigidbody.linearVelocity = Vector3.zero;
            }
            for (int i = 0; i < 7; i++)
            {
                yield return null;
                foreach (Rigidbody rigidbody in _allRigidbodys)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                }
            }
            _settling = false;
            ApplyPendingPushes();
        }

        private void ApplyPendingPushes()
        {
            foreach (var push in _pendingPushes)
                if (Time.unscaledTime - push.Time <= _pushLifetime)
                    Apply(push);

            _pendingPushes.Clear();
        }

        private void Apply(PendingPush push)
        {
            foreach (Rigidbody rigidbody in _allRigidbodys)
                if (rigidbody != null && !rigidbody.isKinematic)
                    rigidbody.AddForce(push.Direction * (_bodySpeed * push.Strength), ForceMode.VelocityChange);

            if (push.Body != null && !push.Body.isKinematic)
                push.Body.AddForce(push.Direction * (_hitBoneSpeed * push.Strength), ForceMode.VelocityChange);
        }

        private IEnumerator Freeze()
        {
            yield return new WaitForSeconds(5);

            foreach (Rigidbody rigidbody in _allRigidbodys)
                rigidbody.isKinematic = true;
        }
    }
}
