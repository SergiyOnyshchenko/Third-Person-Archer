using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Burst.CompilerServices;
using RayFire;

namespace Actor
{
    public class LastProjectileView : System, IActorIniter
    {
        [SerializeField] private CinemachineVirtualCamera _regularCamera;
        [SerializeField] private CinemachineVirtualCamera _lastCamera;

        [SerializeField] private Camera _xRayOverlayCamera;

        private ActorController _actor;
        private ShootingTargets _shootingTargets;
        private ProjectileShooter[] _shooters;

        private Projectile _projectile;

        private IEnumerator _timer;
        private IEnumerator _maxDistanceChecker;

        private ITriggerReciever _currentFreezeEnemy;

        public UnityEvent OnRegularActivated = new UnityEvent();
        public UnityEvent OnLastActivated = new UnityEvent();

        public UnityEvent OnDeactivated = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            _actor = actor;

            if (actor.TryGetProperty(out ShootingTargets shootingTargets))
                _shootingTargets = shootingTargets;

            _shooters = actor.GetComponentsInChildren<ProjectileShooter>();

            foreach (var shooter in _shooters)
            {
                shooter.OnShooted.AddListener(TryShowProjectile);
            }
        }

        private void TryShowProjectile(Projectile projectile)
        {
            RaycastHit projectilePredictiveHit = projectile.GetPredictiveHit();

            if (projectilePredictiveHit.collider != null && projectilePredictiveHit.collider.TryGetComponent(out IDamageChecker damageChecker))
            {
                if (_shootingTargets.Targets.Count == 1 && projectile.PreCheckTargetDeath())
                {
                    ShowLastProjectile(projectile, projectilePredictiveHit);
                }
                else
                {
                    ShowRegularProjectile(projectile, projectilePredictiveHit);
                }
            }
            else
            {

            }
        }

        private void ShowRegularProjectile(Projectile projectile, RaycastHit projectilePredictiveHit)
        {
            if (projectilePredictiveHit.collider != null)
            {
                float distance = Vector3.Distance(projectile.transform.position, projectilePredictiveHit.point);
                StartCheckMaxTravaledDistance(distance + 1f);
            }

            SetProjectileSettings(projectile);
            //SetCameraSettings(projectile.transform);
 
            projectile.GetComponentInChildren<VFXController>()?.Enable();
            projectile.OnHited.AddListener(Deactivate);
            projectile.EnableFeedbacks(false);

            StartDeactivateTimer();

            OnRegularActivated?.Invoke();
        }

        private void ShowLastProjectile(Projectile projectile, RaycastHit projectilePredictiveHit)
        {
            if (projectilePredictiveHit.collider != null && projectilePredictiveHit.collider.TryGetComponent(out ITriggerReciever triggerReciever))
            {
                _currentFreezeEnemy = triggerReciever;
                _currentFreezeEnemy.ReciveTrigger("TimeFreeze", projectile.gameObject);
            }

            _xRayOverlayCamera.gameObject.SetActive(true);

            SetProjectileSettings(projectile);
            //SetCameraSettings(projectile.transform);

            projectile.GetComponentInChildren<VFXController>()?.Enable();
            projectile.OnHited.AddListener(Deactivate);
            projectile.EnableFeedbacks(false);

            StartDeactivateTimer();

            OnLastActivated?.Invoke();
        }

        private void Deactivate()
        {
            //_camera.Follow = null;
            //_camera.LookAt = null;

            StopDeactivateTimer();
            StopCheckMaxTravaledDistance();

            if (_currentFreezeEnemy != null)
            {
                _currentFreezeEnemy.ReciveTrigger("TimeUnfreeze", gameObject);
                _currentFreezeEnemy = null;
            }

            _projectile.OnHited.RemoveListener(Deactivate);
            OnDeactivated?.Invoke();

            DOVirtual.DelayedCall(1f, () =>
            {
                //ResetCameraSettings();
            });
        }

        public void ResetXRayCamera()
        {
            _xRayOverlayCamera.gameObject.SetActive(false);
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
            Deactivate();
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
            do
            {
                if (_projectile == null)
                    yield break;

                yield return new WaitForFixedUpdate();  
            }
            while (_projectile.TraveledDistance < targetDistance);

            Deactivate();
        }

        private void SetProjectileSettings(Projectile projectile)
        {
            _projectile = projectile;

            _projectile.SetDamage(100);
            //_projectile.SetSpeed(3000);
            _projectile.SetSpeed(30);
        }

        private void SetCameraSettings(CinemachineVirtualCamera camera, Transform target)
        {
            camera.transform.SetParent(null);

            camera.Follow = target;
            camera.LookAt = target;

            camera.Priority = 100;
        }

        private void ResetCameraSettings(CinemachineVirtualCamera camera)
        {
            camera.transform.SetParent(_actor.transform);
            camera.Priority = 0;
        }
    }
}