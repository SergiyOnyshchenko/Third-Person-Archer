 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

namespace Actor
{
    public class ElementalController : System, IActorIniter
    {
        private Mana _mana;
        private ElementalArrowsCount _elementalArrowsCount;
        private ElementalAttackType _elementalAttackType;
        public int ElementalArrowsCount { get => _elementalArrowsCount.Value; }
        public UnityEvent OnElementSelected = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
                _elementalAttackType = elementalAttackType;

            if (actor.TryGetProperty(out ElementalArrowsCount elementalArrowsCount))
                _elementalArrowsCount = elementalArrowsCount;

            if (actor.TryGetProperty(out Mana mana))
                _mana = mana;
        }

        public void TrySetElementalType(ElementalType type)
        {
            if (_mana.Ratio < 1f)
                return;

            _mana.Reset();
            _elementalAttackType.SetValue(type);

            _elementalArrowsCount.SetValue(3);

            OnElementSelected?.Invoke();
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
    }
}