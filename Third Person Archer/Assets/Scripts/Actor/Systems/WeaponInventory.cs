using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class WeaponInventory : System, IActorIniter, IWeaponEquipper
    {
        [SerializeField] private WeaponInventoryData _weapons;
        private WeaponData _equipedWeapon;
        private int _weaponIndex = 0;
        private ActorController _actor;
        private PlayerSkinController _skinController;
        public WeaponType EquippedWeaponType { get => _equipedWeapon.Type; }
        public UnityEvent OnWeaponChanged = new UnityEvent();

        private void OnEnable()
        {
            _weapons.OnEquipped += Equip;
        }

        private void OnDisable()
        {
            _weapons.OnEquipped -= Equip;
        }

        public void InitActor(ActorController actor)
        {
            _actor = actor;
            Equip(_weapons.Weapons[_weaponIndex]);

            if(actor.TryGetSystem(out PlayerSkinController skinController))
            {
                _skinController = skinController;
                _skinController.OnSkinChanged.AddListener(Equip);
            }
        }

        public void Equip()
        {
            if (_equipedWeapon == null)
                return;

            Equip(_equipedWeapon);
        }

        public void Equip(WeaponData weapon)
        {
            if (_equipedWeapon != null)
                _equipedWeapon.Unequip(_actor);

            weapon.Equip(_actor);
            _equipedWeapon = weapon;
            OnWeaponChanged?.Invoke();
        }

        public void EquipNext()
        {
            _weaponIndex++;

            if (_weaponIndex >= _weapons.Weapons.Length)
                _weaponIndex = 0;

            Equip(_weapons.Weapons[_weaponIndex]);
        }

        public bool TryEquip(WeaponType type)
        {
            foreach (var weapon in _weapons.Weapons)
            {
                if (weapon.Type == type)
                {
                    Equip(weapon);
                    return true;
                }
            }

            return false;
        }
    }
}
