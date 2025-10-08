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
        private bool _wasDrawing;
        private Transform _shootPoint;
        private TrajectoryProfile _profile;
        private TrajectoryPreviewView _previewView;
        private WeaponPull _weaponPull;
        private ProjectileEnemiesLayermask _enemyLayermask;
        private ITrajectoryPredictor _predictor;
        public ITrajectoryPredictor Predictor => _predictor;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out _previewView)) { }

            if (actor.TryGetProperty(out _weaponPull)) { }
            if (actor.TryGetProperty(out _enemyLayermask)) { }
        }

        public void InitPredictor(TrajectoryProfile profile, Transform shootPoint)
        {
            _profile = profile;
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
                Vector3 origin = SafeSpawnPoint(_shootPoint.position);
                Vector3 dir = ComputeAimDirection(origin);
                float speed = ComputeSpeedFromPull(_weaponPull.Value, _profile);
                Vector3 v0 = dir * speed;

                var predictParams = new TrajectoryPredictParams
                {
                    Origin = origin,
                    InitialVelocity = v0,
                    Gravity = _profile.Gravity,
                    MaxTime = _profile.MaxPreviewTime,
                    MaxSegments = _profile.MaxPreviewSegments,
                    CollisionMask = _profile.CollisionMask
                };

                var prediction = _predictor.Predict(in predictParams);
                _previewView.Render(in prediction);
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

        public bool PredictEnemyHit(out GameObject enemy, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            enemy = null;
            if (_weaponPull == null || _profile == null || _previewView == null) return false;

            Vector3 origin = SafeSpawnPoint(_shootPoint.position);
            Vector3 dir = ComputeAimDirection(origin);
            float speed = ComputeSpeedFromPull(_weaponPull.Value, _profile);
            Vector3 v0 = dir * speed;

            LayerMask enemyMask = _enemyLayermask.Value;
            LayerMask hitMask = _profile.CollisionMask;

            var predictParams = new TrajectoryPredictParams
            {
                Origin = origin,
                InitialVelocity = v0,
                Gravity = _profile.Gravity,
                MaxTime = _profile.MaxPreviewTime,
                MaxSegments = _profile.MaxPreviewSegments,
                CollisionMask = hitMask
            };

            bool willHitEnemy = _predictor.TryHitEnemy(in predictParams, enemyMask, out enemy, out hitPoint);
            return willHitEnemy;
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
            if (Physics.Raycast(ray, out RaycastHit hit, _aimRayDistance, _aimCollisionMask, QueryTriggerInteraction.Ignore))
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