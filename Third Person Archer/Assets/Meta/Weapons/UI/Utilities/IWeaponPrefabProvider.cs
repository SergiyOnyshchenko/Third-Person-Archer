using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    public interface IWeaponPrefabProvider
    {
        GameObject GetPrefab(string weaponId);
    }
}