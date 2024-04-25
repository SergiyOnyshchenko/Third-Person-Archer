using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class CrossbowController : WeaponController, IActorIniter
    {
        [Header("Crossbow View")]
        [SerializeField] private GameObject _crossbow;
        [SerializeField] private CrossbowAnimator _crossbowAnimator;
        [Header("Arrow View")]
        [SerializeField] private GameObject _arrow;
        private FpvController _fpv;
        private AimInput _aimInput;
        private Shooter _shooter;

        private void Start()
        {
            _shooter = GetComponent<Shooter>();
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out FpvController fpv))
                _fpv = fpv;

            if (actor.TryGetInput(out AimInput aimInput))
                _aimInput = aimInput;
        }

        public void ShowView()
        {
            _crossbow.SetActive(true);
        }

        public void Shoot(UnityAction onComplete)
        {
            onComplete += HideArrow;
            onComplete += ShootProjectile;
            _crossbowAnimator.Shoot(onComplete);
        }

        public void Reload()
        {
            _crossbowAnimator.Reload(ShowArrow);
        }

        public void UpdateHands()
        {
            _fpv.LeftHand.transform.position = _crossbowAnimator.LeftHand.position;
            _fpv.LeftHand.transform.rotation = _crossbowAnimator.LeftHand.rotation;

            _fpv.RightHand.transform.position = _crossbowAnimator.RightHand.position;
            _fpv.RightHand.transform.rotation = _crossbowAnimator.RightHand.rotation;
        }

        private void ShootProjectile()
        {
            _shooter.Shoot(_aimInput.GetAimDirection(), 1f);
        }

        private void ShowArrow() => ShowArrow(true);
        private void HideArrow() => ShowArrow(false);
        private void ShowArrow(bool value) => _arrow.SetActive(value);
    }
}

