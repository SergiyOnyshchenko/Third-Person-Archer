using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.Unlocks
{
    public interface IWeaponClassUnlockRepository
    {
        WeaponClassUnlockState Load();
        void Save(WeaponClassUnlockState state);
    }
}
