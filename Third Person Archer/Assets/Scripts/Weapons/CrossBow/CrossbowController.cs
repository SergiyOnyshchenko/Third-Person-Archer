using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
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
        private CrossbowAmmo _ammo;
        public UnityEvent OnShoot = new UnityEvent();

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);

            if (actor.TryGetSystem(out FpvController fpv))
                _fpv = fpv;

            if (actor.TryGetProperty(out CrossbowAmmo ammo))
                _ammoCount = ammo;
        }

        public void SetSettings()
        {
            if (_fpv.FpvAnimator.Properties.TryGetProperty(out SpringPower power))
                power.SetValue(24f);
        }

        public void ShowView()
        {
            _crossbow.SetActive(true);
        }

        public void Shoot(UnityAction onComplete)
        {
            if (!CanAttack())
                return;

            //onComplete += HideArrow;
            //onComplete += ShootProjectile;
            //onComplete += InvokeShootEvent;

            //_crossbowAnimator.Shoot(onComplete);

            HideArrow();
            ShootProjectile();
            InvokeShootEvent();
        }

        public void Reload(UnityAction onReloaded)
        {
            if (!CanAttack())
                return;

            _crossbowAnimator.Reload(ShowArrow + onReloaded);
        }

        public void UpdateHands()
        {
            _fpv.LeftHand.transform.position = _crossbowAnimator.LeftHand.position;
            _fpv.LeftHand.transform.rotation = _crossbowAnimator.LeftHand.rotation;

            _fpv.RightHand.transform.position = _crossbowAnimator.RightHand.position;
            _fpv.RightHand.transform.rotation = _crossbowAnimator.RightHand.rotation;
        }

        public void SetReloadSpeedMult(float mult)
        {
            _crossbowAnimator.SetReloadSpeedMult(mult);
        }

        private void ShootProjectile()
        {
            Shoot(1f, null);
        }

        private void ShowArrow() => ShowArrow(true);
        private void HideArrow() => ShowArrow(false);
        private void ShowArrow(bool value) => _arrow.SetActive(value);

        private void InvokeShootEvent()
        {
            OnShoot?.Invoke();
        }
    }
}

