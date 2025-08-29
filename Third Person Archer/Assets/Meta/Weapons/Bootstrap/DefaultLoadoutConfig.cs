using System;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Default Loadout", fileName = "DefaultLoadout")]
    public class DefaultLoadoutConfig : ScriptableObject
    {
        [Tooltip("Weapons to grant on a fresh profile. First valid entry will be equipped.")]
        public string[] DefaultWeaponIds = Array.Empty<string>();

        [Tooltip("If true, apply defaults only when the profile owns no weapons at all. If false, apply whenever the specific weapon is missing.")]
        public bool OnlyWhenNoWeaponsOwned = true;
    }
}

