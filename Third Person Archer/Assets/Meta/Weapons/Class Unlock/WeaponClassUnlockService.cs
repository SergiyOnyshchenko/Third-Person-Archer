#nullable enable
using UnityEngine;

namespace Meta.Weapons
{
    public sealed class WeaponClassUnlockService : IWeaponClassUnlockService
    {
        private readonly MissionProgressData _progress;

        public int LastUnlockedZoneIndex { get; private set; }
        public int CurrentCompanyLevel { get; private set; }

        public WeaponClassUnlockService(MissionProgressData progress)
        {
            _progress = progress;
            Refresh();
        }

        public void Refresh()
        {
            if (_progress == null)
            {
                LastUnlockedZoneIndex = 0;
                CurrentCompanyLevel = 1;
                return;
            }

            LastUnlockedZoneIndex = _progress.GetLastUnlockedZoneIndex();
            CurrentCompanyLevel = _progress.GetCompanyLevel();
        }

        public bool IsClassUnlocked(WeaponClass weaponClass)
        {
            // Start-unlocked classes
            if (weaponClass == WeaponClass.Bow) return true;
            if (weaponClass == WeaponClass.Crossbow) return true;
            if (weaponClass == WeaponClass.Shuriken) return true;

            // Spear = start of Zone 2 => zone index 1 unlocked
            if (weaponClass == WeaponClass.Spear)
                return LastUnlockedZoneIndex >= 1;

            // Boomerang = start of Zone 3 => zone index 2 unlocked
            if (weaponClass == WeaponClass.Boomerang)
                return LastUnlockedZoneIndex >= 2;

            return true;
        }
    }
}