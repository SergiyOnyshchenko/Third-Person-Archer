using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace Actor
{
    public class NavmeshMover : Mover
    {
        private const float _breakDistanceThreshold = 0.25f;

        [Header("Smoothing (0 = legacy behaviour)")]
        [Tooltip("Brakes into the destination (v = sqrt(2·a·d)) and finishes almost on the point instead of stopping dead 0.25 m before it.")]
        [SerializeField] private bool _smoothArrival;
        [Tooltip("Braking deceleration, m/s². 0 = 75% of the Acceleration property (the agent can't brake harder than it accelerates).")]
        [SerializeField, Min(0f)] private float _arrivalDeceleration;
        [SerializeField, Min(0.01f)] private float _arriveDistance = 0.06f;
        [Tooltip("Turns the body toward the velocity with a spring instead of NavMeshAgent rotation. Max turn rate, °/s; 0 = agent rotates.")]
        [SerializeField, Min(0f)] private float _turnSpeed;
        [SerializeField, Min(0.01f)] private float _turnSmoothTime = 0.12f;

        private NavMeshAgent _agent;
        private IEnumerator _moveProcess;
        private UnityAction OnMoveComplete;
        private bool _isMoving;
        private float _yawVelocity;

        /// <summary>Raised by Move() (after the destination is set).</summary>
        public UnityEvent OnMoveStarted = new UnityEvent();
        public bool IsMoving => _isMoving;
        /// <summary>Exact speed (m/s) driven by an animation clip (start/stop); null = normal acceleration.</summary>
        public float? SpeedOverride { get; set; }
        /// <summary>The clip turns the body itself (turn starts): no velocity turning.</summary>
        public bool SuspendTurning { get; set; }

        public float PlanarSpeed
        {
            get
            {
                Vector3 velocity = _agent.velocity;
                velocity.y = 0f;
                return velocity.magnitude;
            }
        }

        public float RemainingPathDistance => RemainingDistance();

        public override void Move(Transform destination, UnityAction onCompleted = null)
        {
            Move(destination.position, onCompleted);
        }

        public override void Move(Vector3 position, UnityAction onCompleted = null)
        {
            _agent.isStopped = false;

            _agent.speed = _speed.Value;
            _agent.acceleration = _acceleration.Value;

            if (_turnSpeed > 0f)
                _agent.updateRotation = false;

            _isMoving = true;
            OnMoveStarted?.Invoke();

            TargetPosition = position;
            _agent.destination = position;

            OnMoveComplete = onCompleted;

            if (_moveProcess != null)
                StopCoroutine(_moveProcess);

            _moveProcess = MoveProcess(position, onCompleted);

            StartCoroutine(_moveProcess);
        }

        public override void Stop()
        {
            _isMoving = false;
            _agent.isStopped = true;
            OnMovingFinished?.Invoke();
        }

        public void FinishMovingManualy()
        {
            FinishMoving();
        }

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);
            _agent = actor.GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_agent == null)
                return;

            Velocity = _agent.velocity;

            if (!_isMoving || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh || _agent.isStopped)
                return;

            if (SpeedOverride.HasValue)
            {
                _agent.speed = Mathf.Max(0.05f, SpeedOverride.Value);
                _agent.acceleration = 1000f;
            }
            else
            {
                _agent.acceleration = _acceleration.Value;

                if (_smoothArrival)
                    BrakeIntoDestination();
            }

            if (_turnSpeed > 0f && !SuspendTurning)
                TurnTowardVelocity();
        }

        private float RemainingDistance()
        {
            if (_agent.pathPending || !_agent.hasPath)
                return Vector3.Distance(_agent.transform.position, TargetPosition);

            if (!float.IsInfinity(_agent.remainingDistance))
                return _agent.remainingDistance;

            // The agent reports Infinity on long paths: sum the corners ourselves.
            Vector3[] corners = _agent.path.corners;
            float distance = 0f;
            Vector3 from = _agent.transform.position;
            for (int i = 1; i < corners.Length; i++)
            {
                distance += Vector3.Distance(from, corners[i]);
                from = corners[i];
            }
            return distance;
        }

        private void BrakeIntoDestination()
        {
            float deceleration = _arrivalDeceleration > 0f ? _arrivalDeceleration : _acceleration.Value * 0.75f;
            float brakeSpeed = Mathf.Sqrt(2f * deceleration * Mathf.Max(0f, RemainingDistance() - _arriveDistance * 0.5f));

            _agent.speed = Mathf.Min(_speed.Value, Mathf.Max(0.3f, brakeSpeed));
        }

        private void TurnTowardVelocity()
        {
            Vector3 velocity = _agent.velocity;
            velocity.y = 0f;

            if (velocity.sqrMagnitude < 0.04f)
                return;

            float targetYaw = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            float yaw = Mathf.SmoothDampAngle(_agent.transform.eulerAngles.y, targetYaw, ref _yawVelocity, _turnSmoothTime, _turnSpeed);

            _agent.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private IEnumerator MoveProcess(Vector3 targetPosition, UnityAction onCompleted)
        {
            if (_smoothArrival)
            {
                // Wait for the path, then brake until on the point (or stalled right next to it).
                yield return null;
                while (RemainingDistance() > _arriveDistance &&
                       !(RemainingDistance() < _breakDistanceThreshold && _agent.velocity.sqrMagnitude < 0.01f))
                    yield return null;
            }
            else
            {
                while (Vector3.Distance(_agent.transform.position, targetPosition) >= _breakDistanceThreshold)
                    yield return null;
            }

            FinishMoving();
        }

        private void FinishMoving()
        {
            _isMoving = false;
            _agent.speed = _speed.Value;
            _agent.acceleration = _acceleration.Value;

            if (OnMoveComplete != null)
                OnMoveComplete.Invoke();

            OnMovingFinished?.Invoke();
        }
    }
}
