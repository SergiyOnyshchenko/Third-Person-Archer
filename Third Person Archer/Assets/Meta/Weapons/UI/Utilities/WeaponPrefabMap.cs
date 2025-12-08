using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Weapon Prefab Map", fileName = "WeaponPrefabMap")]
    public class WeaponPrefabMap : ScriptableObject, IWeaponPrefabProvider
    {
        [Serializable] public struct Entry { public string WeaponId; public GameObject Prefab; }
        [SerializeField] private List<Entry> entries = new();

        public GameObject GetPrefab(string weaponId)
        {
            foreach (var e in entries) if (e.WeaponId == weaponId) return e.Prefab;
            return null;
        }
    }
}