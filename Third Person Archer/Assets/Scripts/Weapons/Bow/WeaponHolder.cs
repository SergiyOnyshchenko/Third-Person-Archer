 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class WeaponHolder : Property
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private PovType _pov;
        private GameObject _weaponModel;
        public Transform Pivot => _pivot;
        public PovType Pov { get => _pov; }

        public void SetWeapon(GameObject prefab)
        {
            if (_weaponModel != null)
                Destroy(_weaponModel);

            _weaponModel = Instantiate(prefab, _pivot);
        }

        public void ShowWeapon(bool value)
        {
            _pivot.gameObject.SetActive(value);
        }
    }
}