 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class WeaponHolder : Property, IActorIniter
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private PovType _pov;
        private ElementalAttackType _elementalAttackType;
        private GameObject _weaponModel;
        public Transform Pivot => _pivot;
        public PovType Pov { get => _pov; }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
                _elementalAttackType = elementalAttackType;
        }

        public void SetWeapon(GameObject prefab)
        {
            if (_weaponModel != null)
                Destroy(_weaponModel);

            _weaponModel = Instantiate(prefab, _pivot);

            if(_elementalAttackType != null)
            {
                ElementalView[] views = _weaponModel.GetComponentsInChildren<ElementalView>(true);

                foreach (var view in views)
                    view.SetCurrentView(_elementalAttackType.Value);
            }
        }

        public void ShowWeapon(bool value)
        {
            _pivot.gameObject.SetActive(value);
        }
    }
}