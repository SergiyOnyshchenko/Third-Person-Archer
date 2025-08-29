using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    /// <summary>
    /// Wraps your SaveSystem.Save/Load to keep persistence out of domain logic.
    /// </summary>
    public class WeaponRepository : IWeaponRepository
    {
        private const string FileName = "WeaponsState.json";

        public WeaponsState Load()
        {
            var loaded = SaveSystem.Load(FileName, new WeaponsState());
            return loaded ?? new WeaponsState();
        }

        public void Save(WeaponsState state)
        {
            SaveSystem.Save(FileName, state);
        }
    }
}

