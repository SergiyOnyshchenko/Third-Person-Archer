using System;
using UnityEngine;
using UnityEngine.AI;

namespace Actor
{
    /// <summary>
    /// Start / stop animations for NavMesh point-to-point runs. While a clip plays, the agent's speed
    /// follows the clip's root-motion speed curve (baked from the "_Root" FBX variants), so the feet
    /// don't slide. Turn starts (90° / 180°) turn the body inside the clip; at the hand-over the
    /// transform takes that yaw and the run state continues at a matching foot phase.
    /// Data is baked by Tools/Animation/Bake Locomotion Clips.
    /// </summary>
    public class LocomotionClipsDriver : System, IActorIniter
    {
        [Serializable]
        public class MotionClip
        {
            public string State;
            public float Duration;
            [Tooltip("Root speed over time, m/s (humanScale applied).")]
            public AnimationCurve Speed = new AnimationCurve();
            [Tooltip("Yaw the clip turns the body by (deg, + = right); the transform takes it at the hand-over.")]
            public float TurnYaw;
            [Tooltip("Run state normalized time whose pose matches the clip's last frame.")]
            public float ExitPhase;
        }

        [SerializeField] private string _runState = "Movement";

        [Header("Starts: forward, left 90, right 90, left 180, right 180")]
        [SerializeField] private MotionClip _startForward;
        [SerializeField] private MotionClip _startLeft90;
        [SerializeField] private MotionClip _startRight90;
        [SerializeField] private MotionClip _startLeft180;
        [SerializeField] private MotionClip _startRight180;
        [SerializeField, Min(0f)] private float _minStartDistance = 2.5f;
        [SerializeField, Min(0f)] private float _startBlend = 0.12f;

        [Header("Stops: planted left / right foot")]
        [SerializeField] private MotionClip _stopLeft;
        [SerializeField] private MotionClip _stopRight;
        [Tooltip("Run phases with the left foot leading (Jog/Dash: around 0.0).")]
        [SerializeField] private Vector2 _leftLeadPhase = new Vector2(0.75f, 0.25f);
        [SerializeField, Min(0f)] private float _minStopSpeed = 2.2f;
        [SerializeField, Min(0.01f)] private float _stopBlend = 0.2f;

        private enum Phase { None, PendingStart, Start, Run, Stop }

        private NavmeshMover _mover;
        private NavMeshAgent _agent;
        private Transform _root;
        private Animator _animator;
        private Phase _phase;
        private MotionClip _clip;
        private float _time;
        private float _entrySpeed;
        private float _startYaw;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out Mover mover))
                _mover = mover as NavmeshMover;

            if (actor.TryGetSystem(out Animator animator))
                _animator = animator;

            _agent = actor.GetComponent<NavMeshAgent>();
            _root = actor.transform;

            if (_mover != null)
            {
                _mover.OnMoveStarted.AddListener(OnMoveStarted);
                _mover.OnMovingFinished.AddListener(OnMoveFinished);
            }
        }

        private void OnDestroy()
        {
            if (_mover == null)
                return;

            _mover.OnMoveStarted.RemoveListener(OnMoveStarted);
            _mover.OnMovingFinished.RemoveListener(OnMoveFinished);
        }

        private void OnMoveStarted()
        {
            // Hold still for the frame the path takes; the start clip decides the speed.
            ReleaseMover();
            _phase = Phase.PendingStart;
            _mover.SpeedOverride = 0f;
        }

        private void OnMoveFinished()
        {
            ReleaseMover();
            _phase = Phase.None;
        }

        private void ReleaseMover()
        {
            if (_mover == null)
                return;

            _mover.SpeedOverride = null;
            _mover.SuspendTurning = false;
        }

        private void Update()
        {
            if (_mover == null || _animator == null || !_mover.IsMoving)
                return;

            switch (_phase)
            {
                case Phase.PendingStart: TryBeginStart(); break;
                case Phase.Start: UpdateStart(); break;
                case Phase.Run: TryBeginStop(); break;
                case Phase.Stop: UpdateStop(); break;
            }
        }

        // ── Start ──────────────────────────────────────────────────────────────

        private void TryBeginStart()
        {
            if (_agent.pathPending)
                return;

            _mover.SpeedOverride = null;
            _phase = Phase.Run;

            if (!_agent.hasPath || _mover.RemainingPathDistance < _minStartDistance)
                return;

            Vector3 toPath = _agent.steeringTarget - _root.position;
            toPath.y = 0f;
            if (toPath.sqrMagnitude < 0.0001f)
                return;

            float angle = Vector3.SignedAngle(Flat(_root.forward), toPath, Vector3.up);
            MotionClip clip = Mathf.Abs(angle) <= 45f ? _startForward
                : Mathf.Abs(angle) <= 135f ? (angle < 0f ? _startLeft90 : _startRight90)
                : (angle < 0f ? _startLeft180 : _startRight180);

            if (!IsValid(clip))
                return;

            _clip = clip;
            _time = 0f;
            _startYaw = _root.eulerAngles.y;
            _phase = Phase.Start;

            bool turns = Mathf.Abs(clip.TurnYaw) > 1f;
            _mover.SuspendTurning = turns;
            _mover.SpeedOverride = clip.Speed.Evaluate(0f);
            _animator.CrossFade(clip.State, turns ? 0.05f : _startBlend);
        }

        private void UpdateStart()
        {
            _time += Time.deltaTime;
            _mover.SpeedOverride = _clip.Speed.Evaluate(_time);

            bool turns = Mathf.Abs(_clip.TurnYaw) > 1f;
            float handOver = turns ? _clip.Duration : _clip.Duration - _startBlend;
            if (_time < handOver)
                return;

            if (turns)
            {
                // The clip ends facing startYaw + TurnYaw; give that yaw to the transform in the same frame.
                _root.rotation = Quaternion.Euler(0f, _startYaw + _clip.TurnYaw, 0f);
                _animator.Play(_runState, _clip.ExitPhase);
            }
            else
            {
                _animator.CrossFade(_runState, _startBlend);
            }

            ReleaseMover();
            _phase = Phase.Run;
        }

        // ── Stop ───────────────────────────────────────────────────────────────

        private void TryBeginStop()
        {
            float speed = _mover.PlanarSpeed;
            if (speed < _minStopSpeed || !_agent.hasPath || _agent.pathPending)
                return;

            float phase = _animator.GetStatePhase(_runState);
            MotionClip clip = phase >= 0f && IsLeftLead(phase) ? _stopLeft : _stopRight;
            if (!IsValid(clip))
                return;

            if (_mover.RemainingPathDistance > StopDistance(clip, speed))
                return;

            _clip = clip;
            _time = 0f;
            _entrySpeed = speed;
            _phase = Phase.Stop;
            _mover.SpeedOverride = speed;
            _animator.CrossFade(clip.State, _stopBlend);
        }

        private void UpdateStop()
        {
            _time += Time.deltaTime;

            // Past the clip's end the agent creeps onto the point (the clip is already in the stance).
            float speed = _time < _clip.Duration ? StopSpeed(_clip, _entrySpeed, _time) : 0.4f;
            _mover.SpeedOverride = speed;
        }

        private float StopSpeed(MotionClip clip, float entrySpeed, float time)
        {
            float blend = Mathf.SmoothStep(0f, 1f, time / _stopBlend);
            return Mathf.Lerp(entrySpeed, clip.Speed.Evaluate(time), blend);
        }

        private float StopDistance(MotionClip clip, float entrySpeed)
        {
            const int steps = 24;
            float dt = clip.Duration / steps, distance = 0f;
            for (int i = 0; i < steps; i++)
                distance += StopSpeed(clip, entrySpeed, (i + 0.5f) * dt) * dt;
            return distance;
        }

        private bool IsLeftLead(float phase) =>
            _leftLeadPhase.x <= _leftLeadPhase.y
                ? phase >= _leftLeadPhase.x && phase < _leftLeadPhase.y
                : phase >= _leftLeadPhase.x || phase < _leftLeadPhase.y;

        private bool IsValid(MotionClip clip) =>
            clip != null && clip.Duration > 0f && !string.IsNullOrEmpty(clip.State) && _animator.HasState(clip.State);

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }
    }
}
