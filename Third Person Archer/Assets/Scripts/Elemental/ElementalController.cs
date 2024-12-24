 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;
using DG.Tweening;

namespace Actor
{
    public class ElementalController : System, IActorIniter
    {
        [SerializeField] private ElementalData[] _database;
        private bool _isViewActive;
        private ActorController _actor;
        private Mana _mana;
        private ShootingTargets _shootingTargets;
        private ElementalArrowsCount _elementalArrowsCount;
        private ElementalAttackType _elementalAttackType;
        private IsShootingState _isShootingState;
        private ElementalData _activeElementalArrow;
        public ElementalData[] Database { get => _database; }
        public UnityEvent OnElementSelected = new UnityEvent();
        public UnityEvent OnShowView = new UnityEvent();
        public UnityEvent OnHideView = new UnityEvent();


        public void InitActor(ActorController actor)
        {
            _actor = actor;

            if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
                _elementalAttackType = elementalAttackType;

            if (actor.TryGetProperty(out ElementalArrowsCount elementalArrowsCount))
                _elementalArrowsCount = elementalArrowsCount;

            if (actor.TryGetProperty(out IsShootingState isShootingState))
                _isShootingState = isShootingState;

            if (actor.TryGetProperty(out ShootingTargets shootingTargets))
                _shootingTargets = shootingTargets;

            if (actor.TryGetProperty(out Mana mana))
            {
                _mana = mana;
            }  
        }

        private void OnEnable()
        {
            foreach (var data in _database)
            {
                data.OnActivated.AddListener(TryActivateElementalArrow);
            }
        }

        private void OnDisable()
        {
            foreach (var data in _database)
            {
                data.OnActivated.RemoveListener(TryActivateElementalArrow);
            }
        }

        private void Update()
        {
            if (_mana == null)
                return;

            if (_shootingTargets == null)
                return;

            if (_mana.Ratio == 1 && !_isViewActive)
            {
                ShowElementalSelectionView();
            }
        }

        public void TryActivateElementalArrow(ElementalData data)
        {
            if (_activeElementalArrow != null)
                return;

            _activeElementalArrow = data;
            _elementalAttackType.SetValue(data.Type);
            data.RemoveArrow();

            if (AppMetricaEventReporter.Instance != null)
                AppMetricaEventReporter.Instance.SendElementalArrowUsed(_actor, data.Type);
        }

        public void TrySelectElementalType(ElementalType type)
        {
            if (_mana.Ratio < 1f)
                return;

            _mana.Reset();

            foreach (var data in _database)
            {
                if (data.Type == type)
                {
                    data.AddArrows(1);
                    break;
                }
            }

            OnElementSelected?.Invoke();
            OnHideView?.Invoke();

            if (AppMetricaEventReporter.Instance != null)
                AppMetricaEventReporter.Instance.SendElementalArrowObtained(_actor, type);

            _isViewActive = false;
        }

        public void TrySetElementalType(int type)
        {
            TrySelectElementalType((ElementalType)type);
        }

        public void DecreaseElementalArrowCount()
        {
            if (_activeElementalArrow == null)
                return;

            DOVirtual.DelayedCall(0.1f, () =>
            {
                _activeElementalArrow = null;
                _elementalAttackType.SetValue(ElementalType.NULL);
            });
        }

        private void ShowElementalSelectionView()
        {
            _isViewActive = true;
            OnShowView?.Invoke();
        }
    }
}