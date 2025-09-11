using System.Collections;
using System.Collections.Generic;
using System;
using Actor;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

namespace Actor
{
    [Serializable]
    public class CinematicSpeedProfile
    {
        [Tooltip("Speed factor over normalized progress [0..1]. Keep average ~1.")]
        public AnimationCurve SpeedOverProgress = new AnimationCurve(
            new Keyframe(0f, 0.25f, 0f, 2f),   // gentle start
            new Keyframe(0.35f, 0.7f),
            new Keyframe(0.7f, 1.2f),
            new Keyframe(1f, 1f, 0f, 0f)       // settle at ~1x
        );

        [Header("Duration vs Distance")]
        [Tooltip("Approx distance at which we hit MaxDuration.")]
        public float DurationDistance = 60f;
        public float MinDuration = 0.35f;
        public float MaxDuration = 1.10f;

        [Header("Clamps (m/s)")]
        public float MinStartSpeed = 20f;
        public float MaxEndSpeed = 120f;

        [Header("Safety")]
        public float MinTargetDistance = 2f;  // avoid divide-by-zero for point-blank
        public int CurveSamples = 64;         // used to normalize the curve
    }

    [Serializable]
    public class ProjectileView : IActorIniter
    {
        [SerializeField] private CinemachineVirtualCamera _camera;
        [SerializeField] private float _afterHitCameraDelay;

        private ActorController _actor;
        private Projectile _projectile;

        private float _projectileSpeed = 16000;

        public float AfterHitDelay { get => _afterHitCameraDelay; }

        public UnityEvent OnShoot = new UnityEvent();
        public UnityEvent OnHit = new UnityEvent();
        public UnityEvent OnReset = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            _actor = actor;
        }

        public virtual void ShootHandler(Projectile projectile, RaycastHit predictiveHit, RaycastHit projectilePredictiveHit)
        {
            SetProjectileSettings(projectile);
            SetCameraSettings(projectile.transform);

            projectile.GetComponentInChildren<VFXController>()?.Enable();
            projectile.EnableFeedbacks(false);

            OnShoot?.Invoke();
        }

        public virtual void HitHandler()
        {
            OnHit?.Invoke();
        }

        public virtual void ResetHandler()
        {
            _camera.Follow = null;
            _camera.LookAt = null;

            ResetCameraSettings();

            OnReset?.Invoke();
        }

        private void SetCameraSettings(Transform target)
        {
            _camera.transform.SetParent(null);

            _camera.Follow = target;
            _camera.LookAt = target;

            _camera.Priority = 100;
        }

        private void ResetCameraSettings()
        {
            //_camera.transform.SetParent(_actor.transform);
            _camera.Priority = 0;
        }

        protected virtual void SetProjectileSettings(Projectile projectile)
        {
            _projectile = projectile;

            float speed = 30f;
            projectile.SetSpeed(speed);
        }
    }

    [Serializable]
    public class XRayProjectileView : ProjectileView
    {
        [SerializeField] private Camera _xRayOverlayCamera;
        private ITriggerReciever _currentFreezeEnemy;

        public override void ShootHandler(Projectile projectile, RaycastHit predictiveHit, RaycastHit projectilePredictiveHit)
        {
            base.ShootHandler(projectile, predictiveHit, projectilePredictiveHit);

            if (projectilePredictiveHit.collider != null && projectilePredictiveHit.collider.TryGetComponent(out ITriggerReciever triggerReciever))
            {
                _currentFreezeEnemy = triggerReciever;
                _currentFreezeEnemy.ReciveTrigger("TimeFreeze", projectile.gameObject);
            }

            _xRayOverlayCamera.gameObject.SetActive(true);
        }

        public override void ResetHandler()
        {
            base.ResetHandler();

            if (_currentFreezeEnemy != null)
            {
                _currentFreezeEnemy.ReciveTrigger("TimeUnfreeze", null);
                _currentFreezeEnemy = null;
            }
        }

        protected override void SetProjectileSettings(Projectile projectile)
        {
            base.SetProjectileSettings(projectile);

            projectile.SetDamage(100);
        }
    }

    public class ProjectileCameraController : Actor.System, IActorIniter
    {
        [SerializeField] private ProjectileView _regularView;
        [SerializeField] private XRayProjectileView _xRayView;
        [SerializeField] private CinematicSpeedProfile _cinematicProfile = new CinematicSpeedProfile();
        private IEnumerator _cinematicSpeedRoutine;

        private ActorController _actor;
        private ShootingTargets _shootingTargets;
        private ProjectileShooter[] _shooters;

        private ProjectileView _currentView;
        private Projectile _projectile;

        private IEnumerator _timer;
        private IEnumerator _maxDistanceChecker;
  

        public void InitActor(ActorController actor)
        {
            _actor = actor;

            if (actor.TryGetProperty(out ShootingTargets shootingTargets))
                _shootingTargets = shootingTargets;

            _shooters = actor.GetComponentsInChildren<ProjectileShooter>();

            foreach (var shooter in _shooters)
            {
                shooter.OnShooted.AddListener(ManageShootedProjectile);
            }
        }

        private void ManageShootedProjectile(Projectile projectile)
        {
            _projectile = projectile;

            RaycastHit projectilePredictiveHit = projectile.GetPredictiveHit();

            if (projectilePredictiveHit.collider != null && projectilePredictiveHit.collider.TryGetComponent(out IDamageChecker damageChecker))
            {
                if (projectile.PreCheckTargetDeath())
                {
                    if (_shootingTargets.Targets.Count == 1)
                    {
                        ShootXRayProjectileHandler(projectile, projectilePredictiveHit);
                    }
                    else
                    {
                        //ShootRegularProjectileHandler(projectile, projectilePredictiveHit);
                    }
                }
            }
        }

        private void ShootRegularProjectileHandler(Projectile projectile, RaycastHit projectilePredictiveHit)
        {
            ShootProjectileHandler(_regularView, projectile, projectilePredictiveHit);
        }

        private void ShootXRayProjectileHandler(Projectile projectile, RaycastHit projectilePredictiveHit)
        {
            ShootProjectileHandler(_xRayView, projectile, projectilePredictiveHit);
        }

        private void ShootProjectileHandler(ProjectileView view, Projectile projectile, RaycastHit projectilePredictiveHit)
        {
            _currentView = view;
            _currentView.ShootHandler(projectile, projectilePredictiveHit, projectilePredictiveHit);

            StartDeactivateTimer();

            if (projectilePredictiveHit.collider != null)
            {
                float distance = Vector3.Distance(projectile.transform.position, projectilePredictiveHit.point);
                const float smallOffset = 1f;
                StartCinematicSpeed(distance + smallOffset);
            }

            _projectile.OnHited.AddListener(HitProjectileHandler);
        }

        private void HitProjectileHandler()
        {
            _projectile.OnHited.RemoveListener(HitProjectileHandler);
            StopDeactivateTimer();
            StopCinematicSpeed();

            if (_currentView != null)
            {
                _currentView.HitHandler();
                DOVirtual.DelayedCall(_currentView.AfterHitDelay, () => _currentView.ResetHandler());
            }
        }

        private void StartCinematicSpeed(float targetDistance)
        {
            StopCinematicSpeed();
            _cinematicSpeedRoutine = CinematicSpeedRoutine(targetDistance);
            StartCoroutine(_cinematicSpeedRoutine);
        }

        private void StopCinematicSpeed()
        {
            if (_cinematicSpeedRoutine != null)
                StopCoroutine(_cinematicSpeedRoutine);
            _cinematicSpeedRoutine = null;
        }

        private void StartDeactivateTimer()
        {
            StopDeactivateTimer();
            _timer = DeactivateTimer();
            StartCoroutine(_timer);
        }

        private void StopDeactivateTimer()
        {
            if (_timer != null)
                StopCoroutine(_timer);
        }

        private IEnumerator DeactivateTimer()
        {
            yield return new WaitForSeconds(3f);

            if (_currentView != null)
                _currentView.ResetHandler();
        }

        private IEnumerator CinematicSpeedRoutine(float targetDistance)
        {
            if (_projectile == null) yield break;

            float d = Mathf.Max(targetDistance, _cinematicProfile.MinTargetDistance);

            // pick duration from distance
            float t01 = Mathf.InverseLerp(0f, _cinematicProfile.DurationDistance, d);
            float T = Mathf.Lerp(_cinematicProfile.MinDuration, _cinematicProfile.MaxDuration, t01);

            // to keep world speed constant during slow-mo
            float InvTs() => 1f / Mathf.Max(0.01f, Time.timeScale);

            // start slow but ensure we can still accelerate enough to cover distance in ~T
            float vAvg = d / T;
            float v0 = Mathf.Min(_cinematicProfile.MinStartSpeed, 0.8f * vAvg); // <= 80% of avg speed
            float a = Mathf.Max(0f, 2f * (d - v0 * T) / (T * T));              // constant accel (m/s^2)

            float t = 0f;
            _projectile.SetSpeed(v0 * InvTs());

            while (_projectile != null)
            {
                t += Time.unscaledDeltaTime;

                float v = v0 + a * t; // linearly ramp up
                v = Mathf.Min(v, _cinematicProfile.MaxEndSpeed);

                _projectile.SetSpeed(v * InvTs());

                if (_projectile.TraveledDistance >= d || t >= T * 1.2f) break;
                yield return null;
            }

            _currentView?.ResetHandler();
        }
    }
}