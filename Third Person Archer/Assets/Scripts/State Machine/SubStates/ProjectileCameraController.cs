using System.Collections;
using System.Collections.Generic;
using System;
using Actor;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;
using static RayFire.RayfireBomb;

namespace Actor
{
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

            projectile.SetSpeed(16000);
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
                        ShootRegularProjectileHandler(projectile, projectilePredictiveHit);
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
                float offset = 1f;
                StartCheckMaxTravaledDistance(distance + offset);
            }

            _projectile.OnHited.AddListener(HitProjectileHandler);
        }

        private void HitProjectileHandler()
        {
            _projectile.OnHited.RemoveListener(HitProjectileHandler);

            StopDeactivateTimer();
            StopCheckMaxTravaledDistance();

            if (_currentView != null)
            {
                _currentView.HitHandler();

                DOVirtual.DelayedCall(_currentView.AfterHitDelay, () =>
                {
                    _currentView.ResetHandler();
                });
            }
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
            yield return new WaitForSeconds(1.25f);

            if (_currentView != null)
                _currentView.ResetHandler();
        }

        private void StartCheckMaxTravaledDistance(float targetDistance)
        {
            StopCheckMaxTravaledDistance();
            _maxDistanceChecker = MaxTravaledDistanceChecker(targetDistance);
            StartCoroutine(_maxDistanceChecker);
        }

        private void StopCheckMaxTravaledDistance()
        {
            if (_maxDistanceChecker != null)
                StopCoroutine(_maxDistanceChecker);
        }

        private IEnumerator MaxTravaledDistanceChecker(float targetDistance)
        {
            float startSpeed = _projectile.Speed.Value;
            float maxSpeed = startSpeed * 5f;

            do
            {
                if (_projectile == null)
                    yield break;

                float t = Mathf.InverseLerp(0, targetDistance, _projectile.TraveledDistance);
                float currentSpeed = Mathf.Lerp(startSpeed, maxSpeed, t);

                _projectile.SetSpeed(currentSpeed);

                yield return new WaitForFixedUpdate();
            }
            while (_projectile.TraveledDistance < targetDistance);

            if (_currentView != null)
                _currentView.ResetHandler();
        }
    }
}