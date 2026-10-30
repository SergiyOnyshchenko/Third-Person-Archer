using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    /// <summary>
    /// Soft aim assist for touch aiming. Never aims on its own:
    /// friction slows the camera near an enemy, magnetism bends the player's own swipe toward it,
    /// and release correction fixes a near miss (1–2°) when the shot is fired.
    /// The miss is measured between the enemy body and the real flight path (the predicted arc, or the camera ray for direct weapons).
    /// </summary>
    public class AimAssist : System, IActorIniter
    {
        private struct AssistTarget
        {
            public ActorController Actor;
            public Health Health;
            public List<Collider> Body;
        }

        [SerializeField] private Camera _camera;
        [Tooltip("Strength of the active weapon (set by AimAssistSubstate); 0 = off.")]
        [SerializeField, Min(0f)] private float _strength;

        [Header("Targets")]
        [SerializeField, Min(1f)] private float _range = 80f;
        [SerializeField, Min(0.05f)] private float _refreshInterval = 0.25f;
        [Tooltip("Half width of an enemy body; the miss angle is measured to its edge.")]
        [SerializeField, Min(0f)] private float _bodyRadius = 0.25f;
        [Tooltip("Enemies behind these layers get no assist.")]
        [SerializeField] private LayerMask _occlusionMask = (1 << 3) | (1 << 8) | (1 << 17);

        [Header("Friction")]
        [Tooltip("Miss angle (degrees) where the camera starts to slow down.")]
        [SerializeField, Min(0.1f)] private float _frictionAngle = 5f;
        [Tooltip("Sensitivity lost on the target at strength 1.")]
        [SerializeField, Range(0f, 0.9f)] private float _friction = 0.45f;

        [Header("Magnetism")]
        [SerializeField, Min(0.1f)] private float _magnetAngle = 4f;
        [Tooltip("Degrees of pull toward the target per degree of the player's own swipe, at strength 1.")]
        [SerializeField, Min(0f)] private float _magnet = 0.3f;

        [Header("Release correction")]
        [Tooltip("Largest miss (degrees, at strength 1) the shot is quietly corrected for. Aims at the body, so headshots stay earned.")]
        [SerializeField, Min(0f)] private float _releaseAngle = 1.5f;

        private readonly List<AssistTarget> _targets = new List<AssistTarget>();
        private readonly Dictionary<ActorController, List<Collider>> _grouping = new Dictionary<ActorController, List<Collider>>();
        private readonly Collider[] _overlap = new Collider[128];
        private Vector3[] _path = new Vector3[2];
        private int _pathCount;
        private int _pathFrame = -10;
        private float _nextRefresh;
        private FpvInput _input;
        private ProjectileEnemiesLayermask _enemyMask;

        private bool _hasBest;
        private Vector2 _bestError;
        private float _bestAngle;

        public float Strength { get => _strength; set => _strength = Mathf.Max(0f, value); }
        public bool IsActive => _strength > 0f && _camera != null && _enemyMask != null;

        public void InitActor(ActorController actor)
        {
            actor.TryGetInput(out _input);
            actor.TryGetProperty(out _enemyMask);

            if (_camera == null)
                _camera = Camera.main;
        }

        /// <summary>The flight path for this frame (the predicted arc). Without it the camera ray is used.</summary>
        public void SetPath(in TrajectoryPrediction prediction)
        {
            if (prediction.PointCount < 2)
                return;

            if (_path.Length < prediction.PointCount)
                _path = new Vector3[prediction.PointCount];

            global::System.Array.Copy(prediction.Points, _path, prediction.PointCount);
            _pathCount = prediction.PointCount;
            _pathFrame = Time.frameCount;
        }

        /// <summary>
        /// Called by the camera every frame with the player's rotation this frame (degrees, x = yaw right, y = pitch up).
        /// Updates friction and returns the extra rotation from magnetism.
        /// </summary>
        public Vector2 Tick(Vector2 playerDelta)
        {
            Evaluate();

            float friction = 1f;
            Vector2 magnet = Vector2.zero;

            if (_hasBest)
            {
                float closeness = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, _frictionAngle, _bestAngle));
                friction = 1f - Mathf.Clamp01(_friction * _strength) * closeness;

                float swipe = playerDelta.magnitude;
                float error = _bestError.magnitude;
                if (swipe > 0f && error > 0.0001f && _bestAngle < _magnetAngle)
                {
                    // Pull harder when the swipe already goes toward the target, barely when it leaves it.
                    float toward = Vector2.Dot(playerDelta / swipe, _bestError / error);
                    float weight = Mathf.Clamp01(0.25f + 0.75f * toward);
                    float falloff = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_magnetAngle * 0.5f, _magnetAngle, _bestAngle));
                    float pull = Mathf.Min(error, _magnet * _strength * swipe * weight * falloff);
                    magnet = _bestError / error * pull;
                }
            }

            if (_input != null)
                _input.SensitivityMultiplier = friction;

            return magnet;
        }

        public void Release()
        {
            _hasBest = false;
            if (_input != null)
                _input.SensitivityMultiplier = 1f;
        }

        /// <summary>
        /// Enemy whose body the path passes within the release angle of (seen from the camera), or null.
        /// </summary>
        public bool TryGetReleaseTarget(Vector3[] points, int count, out ActorController actor, out Vector3 body)
        {
            actor = null;
            body = default;
            if (!IsActive || _releaseAngle <= 0f)
                return false;

            RefreshTargets(force: false);
            float limit = _releaseAngle * _strength;
            float best = float.MaxValue;
            Vector3 eye = _camera.transform.position;

            foreach (var target in _targets)
            {
                if (!IsValid(target, out Vector3 center))
                    continue;

                float angle = MissAngle(eye, center, ClosestPoint(points, count, center));
                if (angle <= limit && angle < best)
                {
                    best = angle;
                    actor = target.Actor;
                    body = center;
                }
            }

            return actor != null;
        }

        /// <summary>Straight shot from <paramref name="origin"/>: aims at the body of an enemy just missed.</summary>
        public Vector3 CorrectDirect(Vector3 origin, Vector3 direction)
        {
            if (!IsActive)
                return direction;

            Vector3 eye = _camera.transform.position;
            var ray = new[] { eye, eye + _camera.transform.forward * _range };
            if (Physics.Raycast(eye, _camera.transform.forward, _range, _enemyMask.Value, QueryTriggerInteraction.Ignore))
                return direction;

            if (!TryGetReleaseTarget(ray, 2, out _, out Vector3 body))
                return direction;

            Vector3 corrected = body - origin;
            return corrected.sqrMagnitude > 0.0001f ? corrected.normalized : direction;
        }

        public static Vector3 ClosestPoint(Vector3[] points, int count, Vector3 to)
        {
            Vector3 best = points[0];
            float bestSqr = float.MaxValue;
            for (int i = 1; i < count; i++)
            {
                Vector3 a = points[i - 1];
                Vector3 ab = points[i] - a;
                float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(to - a, ab) / ab.sqrMagnitude) : 0f;
                Vector3 p = a + ab * t;
                float sqr = (p - to).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = p;
                }
            }
            return best;
        }

        private void OnDisable()
        {
            Release();
        }

        private void Evaluate()
        {
            _hasBest = false;
            if (!IsActive)
                return;

            RefreshTargets(force: false);
            if (_targets.Count == 0)
                return;

            Transform eye = _camera.transform;
            if (Time.frameCount - _pathFrame > 2)
            {
                _path[0] = eye.position;
                _path[1] = eye.position + eye.forward * _range;
                _pathCount = 2;
            }

            float best = float.MaxValue;
            foreach (var target in _targets)
            {
                if (!IsValid(target, out Vector3 center))
                    continue;

                Vector3 closest = ClosestPoint(_path, _pathCount, center);
                float angle = MissAngle(eye.position, center, closest);
                if (angle >= best || angle > Mathf.Max(_frictionAngle, _magnetAngle))
                    continue;

                best = angle;
                _bestAngle = angle;
                _bestError = ViewAngles(eye, center) - ViewAngles(eye, closest);
                _hasBest = true;
            }
        }

        // Angle (degrees) between where the path passes and the edge of the body, seen from the camera.
        private float MissAngle(Vector3 eye, Vector3 center, Vector3 closest)
        {
            float distance = Vector3.Distance(eye, center);
            if (distance < 0.01f)
                return 0f;

            float angle = Vector3.Angle(closest - eye, center - eye);
            float radius = Mathf.Atan2(_bodyRadius, distance) * Mathf.Rad2Deg;
            return Mathf.Max(0f, angle - radius);
        }

        // Yaw (right) / pitch (up) of a point in the camera's frame, in degrees.
        private static Vector2 ViewAngles(Transform eye, Vector3 point)
        {
            Vector3 v = eye.InverseTransformDirection(point - eye.position);
            float yaw = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Atan2(v.y, new Vector2(v.x, v.z).magnitude) * Mathf.Rad2Deg;
            return new Vector2(yaw, pitch);
        }

        private bool IsValid(in AssistTarget target, out Vector3 center)
        {
            center = default;
            if (target.Actor == null || target.Actor.IsDead || (target.Health != null && target.Health.IsDead) || !target.Actor.gameObject.activeInHierarchy)
                return false;

            var bounds = new Bounds();
            bool any = false;
            foreach (var collider in target.Body)
            {
                if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
                    continue;

                if (any) bounds.Encapsulate(collider.bounds);
                else bounds = collider.bounds;
                any = true;
            }

            if (!any)
                return false;

            center = bounds.center;
            Vector3 eye = _camera.transform.position;
            return !Physics.Linecast(eye, center, _occlusionMask, QueryTriggerInteraction.Ignore);
        }

        private void RefreshTargets(bool force)
        {
            if (!force && Time.unscaledTime < _nextRefresh)
                return;
            _nextRefresh = Time.unscaledTime + _refreshInterval;

            _targets.Clear();
            _grouping.Clear();

            int count = Physics.OverlapSphereNonAlloc(_camera.transform.position, _range, _overlap, _enemyMask.Value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                // Body only: the assist never pulls to the head.
                if (!_overlap[i].TryGetComponent(out Hitbox hitbox) || !hitbox.IsTarget || hitbox.IsCritical || hitbox.Actor == null)
                    continue;

                if (!_grouping.TryGetValue(hitbox.Actor, out var colliders))
                {
                    colliders = new List<Collider>();
                    _grouping.Add(hitbox.Actor, colliders);
                    hitbox.Actor.TryGetSystem(out Health health);
                    _targets.Add(new AssistTarget { Actor = hitbox.Actor, Health = health, Body = colliders });
                }
                colliders.Add(_overlap[i]);
            }
        }
    }
}
