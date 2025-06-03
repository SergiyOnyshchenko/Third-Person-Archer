using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine.Events;
using UnityEngine;
using Cinemachine;
using static RayFire.RayfireBomb;
using DG.Tweening;

namespace Actor
{
    public class GuidedProjectileSystem : System, IActorIniter
    {
        [SerializeField] private bool _isActive;
        [Space]
        [SerializeField] private CinemachineVirtualCamera _camera;
        [Space]
        [SerializeField] private GameObject _crosshairUI;
        [SerializeField] private GameObject _attackUI;

        private Projectile _guidedProjectile;

        private ActorController _actor;
        private ProjectileShooter[] _shooters;

        private PlayerAttackInput _attackInput;
        private AimInput _aimInput;

        public UnityEvent OnStarted = new UnityEvent();
        public UnityEvent OnFinished = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            _actor = actor;

            _shooters = actor.GetComponentsInChildren<ProjectileShooter>();

            if (actor.TryGetInput(out _attackInput)) { }
            if (actor.TryGetInput(out _aimInput)) { }

            foreach (var shooter in _shooters)
            {
                shooter.OnShooted.AddListener(TryGuideProjectile);
            }
        }

        private void TryGuideProjectile(Projectile projectile)
        {
            if (_isActive)
            {
                StartGuiding(projectile);
            }
        }

        private void StartGuiding(Projectile projectile)
        {
            _attackInput.FreezeAttack();

            _guidedProjectile = projectile;
            _guidedProjectile.OnHited.AddListener(FinishGuiding);

            if (projectile.gameObject.TryGetComponent(out Lifetime lifetime))
                lifetime.enabled = false;

            _camera.transform.position = _guidedProjectile.transform.position;
            _camera.transform.rotation = _guidedProjectile.transform.rotation;

            _camera.Follow = projectile.transform;
            _camera.LookAt = projectile.transform;
            _camera.Priority = 100;

            projectile.GetComponentInChildren<VFXController>()?.Enable();
            _crosshairUI.SetActive(false);
            _attackUI.SetActive(false);

            OnStarted?.Invoke();

            DOVirtual.DelayedCall(0.25f, () =>
            {
                if (projectile.Actor.TryGetInput(out FpvInput input))
                    input.Activate(true);
            });
        }

        private void FinishGuiding()
        {
            if (_guidedProjectile.Actor.TryGetInput(out FpvInput input))
                input.Activate(false);

            _crosshairUI.SetActive(true);
            _attackUI.SetActive(true);

            _guidedProjectile.OnHited.RemoveListener(FinishGuiding);

            DOVirtual.DelayedCall(0.75f, () =>
            {
                _camera.Priority = 0;
                _camera.Follow = null;
                _camera.LookAt = null;

                _guidedProjectile = null;

                OnFinished?.Invoke();
            });

            DOVirtual.DelayedCall(1.5f, () => 
            {
                _attackInput.UnfreezeAttack();
            });
        }
    }
}