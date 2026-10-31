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
        [Tooltip("Speed (m/s) at which overlapping bones are pushed apart. Physics' default (10) launches the body " +
                 "when it goes limp in deep slow motion, where the tiny time step makes every overlap resolve at full speed.")]
        [SerializeField] private float _maxDepenetration = 1f;

        private struct PendingPush
        {
            public Rigidbody Body;
            public Vector3 Direction;
            public float Strength;
            public float Time;
        }

        private readonly List<PendingPush> _pendingPushes = new List<PendingPush>();
        private bool _settling;
        private float _pushScale = 1f;
        private float _lift = 1f;
        private float _spin;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<UnityEngine.Animator>();

            //_allRigidbodys = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rigidbody in _allRigidbodys)
                if (rigidbody != null)
                    rigidbody.maxDepenetrationVelocity = _maxDepenetration;

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

        /// <summary>
        /// Goes physical with the death impulse scaled by <paramref name="pushScale"/>, its upward part by
        /// <paramref name="lift"/>, and the body spun about the hit's sideways axis by <paramref name="spin"/> deg/s
        /// (cinematic kill shot).
        /// </summary>
        public void MakePhysical(float pushScale, float lift, float spin)
        {
            if (_isPhysical)
                return;

            _pushScale = pushScale;
            _lift = lift;
            _spin = spin;
            MakePhysical();
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
            float strength = push.Strength * _pushScale;
            push.Direction.y *= _lift;
            Vector3 spinAxis = Vector3.Cross(Vector3.up, push.Direction);
            Vector3 spin = spinAxis.sqrMagnitude > 0.0001f ? spinAxis.normalized * (_spin * Mathf.Deg2Rad) : Vector3.zero;
            Vector3 centre = spin != Vector3.zero ? Centre() : Vector3.zero;

            foreach (Rigidbody rigidbody in _allRigidbodys)
                if (rigidbody != null && !rigidbody.isKinematic)
                {
                    // The spin turns the whole body as one: each bone also moves around the body's centre.
                    Vector3 tumble = Vector3.Cross(spin, rigidbody.worldCenterOfMass - centre);
                    rigidbody.AddForce(push.Direction * (_bodySpeed * strength) + tumble, ForceMode.VelocityChange);
                    rigidbody.AddTorque(spin, ForceMode.VelocityChange);
                }

            if (push.Body != null && !push.Body.isKinematic)
                push.Body.AddForce(push.Direction * (_hitBoneSpeed * strength), ForceMode.VelocityChange);
        }

        private Vector3 Centre()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (Rigidbody rigidbody in _allRigidbodys)
                if (rigidbody != null)
                {
                    sum += rigidbody.worldCenterOfMass * rigidbody.mass;
                    mass += rigidbody.mass;
                }
            return mass > 0f ? sum / mass : transform.position;
        }

        private IEnumerator Freeze()
        {
            yield return new WaitForSeconds(5);

            foreach (Rigidbody rigidbody in _allRigidbodys)
                rigidbody.isKinematic = true;
        }
    }
}
