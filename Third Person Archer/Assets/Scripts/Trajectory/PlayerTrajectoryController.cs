using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class PlayerTrajectoryController : System, IActorIniter
    {
        public enum PreviewOriginMode
        {
            World,
            ReprojectFromWeaponCam
        }

        [Header("Camera")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _weaponCamera;
        [SerializeField] private PreviewOriginMode _originMode = PreviewOriginMode.ReprojectFromWeaponCam;
        [Header("Aim")]
        [SerializeField] private float _aimRayDistance = 200f;
        [SerializeField] private LayerMask _aimCollisionMask;
        [SerializeField] private float _minSpawnSeparation = 0.05f;
        [Header("Enemy Preview")]
        [SerializeField] private LayerMask _enemyDetectionMask;
        private bool _wasDrawing;
        private Transform _shootPoint;
        private TrajectoryProfile _profile;
        private TrajectoryPreviewView _previewView;
        private WeaponPull _weaponPull;
        private ProjectileEnemiesLayermask _enemyLayermask;
        private ITrajectoryPredictor _predictor;
        private float _readyPull;
        private float _minLaunchPull;
        private int _projectileMask = ~0;
        private AimAssist _aimAssist;
        public ITrajectoryPredictor Predictor => _predictor;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out _previewView)) { }

            if (actor.TryGetProperty(out _weaponPull)) { }
            if (actor.TryGetProperty(out _enemyLayermask)) { }
            actor.TryGetSystem(out _aimAssist);
        }

        public void InitPredictor(TrajectoryProfile profile, Transform shootPoint, float readyPull = 0f, float minLaunchPull = 0f, int projectileLayer = -1)
        {
            _profile = profile;
            _readyPull = Mathf.Clamp01(readyPull);
            _minLaunchPull = Mathf.Clamp01(minLaunchPull);
            _projectileMask = CollisionMaskFor(projectileLayer);
            _predictor = new AnalyticalTrajectoryPredictor(profile.MaxPreviewSegments + 8);
            _shootPoint = shootPoint;

            if (_profile != null && _profile.ImpactMarkerPrefab != null && _previewView != null)
            {
                _previewView.EnsureMarker(_profile.ImpactMarkerPrefab);
            }
        }

        public void SetOriginMode(PreviewOriginMode mode)
        {
            _originMode = mode;
        }

        public void UpdateTrajectory()
        {
            if (_weaponPull == null || _profile == null || _previewView == null) return;

            if (_weaponPull.Value > 0)
            {
                var prediction = _predictor.Predict(BuildParams(_profile.CollisionMask & _projectileMask));
                AimTarget target = Classify(in prediction);
                float readiness = _readyPull > 0f ? Mathf.Clamp01(_weaponPull.Value / _readyPull) : 1f;

                _previewView.Render(in prediction, target, readiness);
                if (_aimAssist != null)
                    _aimAssist.SetPath(in prediction);
                _wasDrawing = true;
            }
            else
            {
                _previewView.Clear();
                _wasDrawing = false;
            }
        }

        public void Reset()
        {
            _previewView.Clear();
        }

        public bool PredictEnemyHit(out GameObject enemy, out Vector3 hitPoint) =>
            PredictEnemyHit(ComputeAimDirection(SafeSpawnPoint(_shootPoint.position)), out enemy, out hitPoint);

        public bool PredictEnemyHit(Vector3 direction, out GameObject enemy, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            enemy = null;
            if (_weaponPull == null || _profile == null || _previewView == null) return false;

            return _predictor.TryHitEnemy(BuildParams(_profile.CollisionMask & _projectileMask, direction), _enemyLayermask.Value, out enemy, out hitPoint);
        }

        /// <summary>
        /// Launch direction for the shot: the aimed one, quietly corrected onto an enemy body the arc only just misses.
        /// </summary>
        public Vector3 AssistedAimDirection(Vector3 origin)
        {
            Vector3 aimed = ComputeAimDirection(origin);
            if (_aimAssist == null || !_aimAssist.IsActive || _profile == null)
                return aimed;

            LayerMask mask = _profile.CollisionMask & _projectileMask;
            var prediction = _predictor.Predict(BuildParams(mask, aimed));
            if (Classify(in prediction) != AimTarget.None)
                return aimed;

            if (!_aimAssist.TryGetReleaseTarget(prediction.Points, prediction.PointCount, out ActorController actor, out Vector3 body))
                return aimed;

            // Turn the launch so the closest point of the arc lands on the body; a few passes absorb the arc's bend.
            Vector3 dir = aimed;
            for (int i = 0; i < 3; i++)
            {
                Vector3 closest = AimAssist.ClosestPoint(prediction.Points, prediction.PointCount, body);
                dir = Quaternion.FromToRotation(closest - origin, body - origin) * dir;
                prediction = _predictor.Predict(BuildParams(mask, dir));

                if (HitsActor(in prediction, actor))
                    return dir;
            }

            return aimed;
        }

        private static bool HitsActor(in TrajectoryPrediction prediction, ActorController actor)
        {
            return prediction.Hit && prediction.HitInfo.collider != null &&
                   prediction.HitInfo.collider.TryGetComponent(out Hitbox hitbox) && hitbox.Actor == actor;
        }

        /// <summary>
        /// Launch speed for the current draw: a stronger draw throws further. The shooter, the preview and
        /// the hit prediction all use it, so the arc is exactly where the projectile will fly.
        /// </summary>
        public float LaunchSpeed(TrajectoryProfile profile)
        {
            float pull = _weaponPull != null ? _weaponPull.Value : 1f;
            return ComputeSpeedFromPull(Mathf.Max(pull, _minLaunchPull), profile);
        }

        private TrajectoryPredictParams BuildParams(LayerMask collisionMask) =>
            BuildParams(collisionMask, ComputeAimDirection(SafeSpawnPoint(_shootPoint.position)));

        private TrajectoryPredictParams BuildParams(LayerMask collisionMask, Vector3 direction)
        {
            Vector3 origin = SafeSpawnPoint(_shootPoint.position);

            return new TrajectoryPredictParams
            {
                Origin = origin,
                InitialVelocity = direction * LaunchSpeed(_profile),
                Gravity = _profile.Gravity,
                MaxTime = _profile.MaxPreviewTime,
                MaxSegments = _profile.MaxPreviewSegments,
                CollisionMask = collisionMask
            };
        }

        // Layers the projectile physically collides with (the physics matrix), so the preview doesn't stop
        // on geometry the projectile flies through. -1 = unknown, no filtering.
        private static int CollisionMaskFor(int layer)
        {
            if (layer < 0 || layer > 31)
                return ~0;

            int mask = 0;
            for (int i = 0; i < 32; i++)
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                    mask |= 1 << i;
            return mask;
        }

        private AimTarget Classify(in TrajectoryPrediction prediction)
        {
            if (!prediction.Hit || prediction.HitInfo.collider == null)
                return AimTarget.None;

            Collider collider = prediction.HitInfo.collider;
            if ((_enemyDetectionMask.value & (1 << collider.gameObject.layer)) == 0)
                return AimTarget.None;

            return collider.TryGetComponent(out Hitbox hitbox) && hitbox.IsCritical ? AimTarget.Head : AimTarget.Body;
        }

        public float ComputeSpeedFromPull(float pull01, TrajectoryProfile profile)
        {
            float factor = Mathf.Clamp01(profile.PullToSpeedCurve.Evaluate(Mathf.Clamp01(pull01)));
            return Mathf.Max(0f, profile.MaxSpeed * factor);
        }

        public Vector3 SafeSpawnPoint(Vector3 desired)
        {
            if (_originMode == PreviewOriginMode.World || _weaponCamera == null || _mainCamera == null)
                return desired;

            // Project bow into weapon cam (where the player actually sees it)
            Vector3 screenB = _weaponCamera.WorldToScreenPoint(desired);

            // Ray from main cam through that same screen pixel
            Ray ray = _mainCamera.ScreenPointToRay(screenB);

            // Use the true bow distance from main camera to keep depth consistent
            float depth = Vector3.Dot(desired - _mainCamera.transform.position, _mainCamera.transform.forward);
            depth = Mathf.Max(depth, 0.05f);

            return ray.GetPoint(depth);
        }

        public Vector3 ComputeAimDirection(Vector3 spawnPos)
        {
            Vector3 camPos = _mainCamera.transform.position;
            Vector3 camForward = _mainCamera.transform.forward;
            // Ray from camera to find desired impact point (where reticle points).
            Ray ray = new Ray(camPos, camForward);
            if (Physics.Raycast(ray, out RaycastHit hit, _aimRayDistance, _aimCollisionMask & _projectileMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 toHit = (hit.point - spawnPos);
                return toHit.sqrMagnitude > 0.0001f ? toHit.normalized : camForward;
            }
            else
            {
                // No hit: fire towards far point along camera forward
                Vector3 farPoint = camPos + camForward * _aimRayDistance;
                Vector3 toFar = (farPoint - spawnPos);
                return toFar.sqrMagnitude > 0.0001f ? toFar.normalized : camForward;
            }
        }
    }
}