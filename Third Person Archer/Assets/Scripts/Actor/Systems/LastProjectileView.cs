using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

namespace Actor
{
    public class LastProjectileView : System, IActorIniter
    {
        [SerializeField] private CinemachineVirtualCamera _camera;
        private ActorController _actor;
        private ShootingTargets _shootingTargets;
        private ProjectileShooter[] _shooters;
        private Projectile _projectile;
        private IEnumerator _timer;
        private ITriggerReciever _currentFreezeEnemy;
        public UnityEvent OnActivated = new UnityEvent();
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
            if (_shootingTargets.Targets.Count != 1)
                return;

            if (Vector3.Distance(_actor.transform.position, _shootingTargets.Targets[0].RootPoint.position) < 5)
                return;

            if (projectile.PreCheckTargetDeath())
            {
                Activate(projectile);
            }
        }

        private void Activate(Projectile projectile)
        {
            RaycastHit projectilePredictiveHit = projectile.GetPredictiveHit();

            if (projectilePredictiveHit.collider != null && projectilePredictiveHit.collider.TryGetComponent(out ITriggerReciever triggerReciever))
            {
                _currentFreezeEnemy = triggerReciever;
                _currentFreezeEnemy.ReciveTrigger("TimeFreeze", projectile.gameObject);
            }

            _camera.transform.SetParent(null);

            projectile.SetDamage(100);
            projectile.ChangeSpeed(3000);
            projectile.GetComponentInChildren<VFXController>()?.Enable();

            _camera.Follow = projectile.transform;
            _camera.LookAt = projectile.transform;
            OnActivated?.Invoke();

            _projectile = projectile;
            projectile.OnHited.AddListener(Deactivate);

            if (_timer != null)
                StopCoroutine(_timer);

            _timer = DeactivateTimer();

            StartCoroutine(_timer);
        }

        private void Deactivate()
        {
            if (_timer != null)
                StopCoroutine(_timer);

            if (_currentFreezeEnemy != null)
            {
                _currentFreezeEnemy.ReciveTrigger("TimeUnfreeze", gameObject);
                _currentFreezeEnemy = null;
            }

            _projectile.OnHited.RemoveListener(Deactivate);
            OnDeactivated?.Invoke();

            _camera.Follow = null;
            _camera.LookAt = null;
            _camera.transform.SetParent(_actor.transform);

            transform.localPosition = Vector3.zero;
        }

        private IEnumerator DeactivateTimer()
        {
            yield return new WaitForSeconds(1.25f);
            Deactivate();
        }
    }
}