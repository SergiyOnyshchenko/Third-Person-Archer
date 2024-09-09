 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

namespace Actor
{
    public class ElementalController : System, IActorIniter
    {
        private bool _isViewActive;
        private Mana _mana;
        private ShootingTargets _shootingTargets;
        private ElementalArrowsCount _elementalArrowsCount;
        private ElementalAttackType _elementalAttackType;
        public int ElementalArrowsCount { get => _elementalArrowsCount.Value; }
        public UnityEvent OnElementSelected = new UnityEvent();
        public UnityEvent OnShowView = new UnityEvent();
        public UnityEvent OnHideView = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
                _elementalAttackType = elementalAttackType;

            if (actor.TryGetProperty(out ElementalArrowsCount elementalArrowsCount))
                _elementalArrowsCount = elementalArrowsCount;

            if (actor.TryGetProperty(out ShootingTargets shootingTargets))
                _shootingTargets = shootingTargets;

            if (actor.TryGetProperty(out Mana mana))
            {
                _mana = mana;
            }  
        }

        private void Update()
        {
            if (_mana == null)
                return;

            if (_shootingTargets == null)
                return;

            if (_mana.Ratio == 1 && _shootingTargets.Count > 0 && !_isViewActive)
            {
                ShowElementalSelectionView();
            }
        }

        public void TrySetElementalType(ElementalType type)
        {
            if (_mana.Ratio < 1f)
                return;

            _mana.Reset();
            _elementalAttackType.SetValue(type);

            _elementalArrowsCount.SetValue(3);

            OnElementSelected?.Invoke();
            OnHideView?.Invoke();

            _isViewActive = false;
        }

        public void TrySetElementalType(int type)
        {
            TrySetElementalType((ElementalType)type);
        }

        public void DecreaseElementalArrowCount()
        {
            if (_elementalArrowsCount.Value <= 0)
                return;

            _elementalArrowsCount.Decrease();

            if (_elementalArrowsCount.Value <= 0)
                _elementalAttackType.SetValue(ElementalType.NULL);
        }

        private void ShowElementalSelectionView()
        {
            _isViewActive = true;
            OnShowView?.Invoke();
        }
    }
}