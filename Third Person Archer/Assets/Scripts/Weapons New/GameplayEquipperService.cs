using System.Collections.Generic;
using Actor;
using Meta.Weapons;

namespace Game.Weapons
{
    /// <summary>Tries registered equippers until one handles the weapon.</summary>
    public sealed class GameplayEquipperService
    {
        private readonly List<IGameplayEquipper> equippers = new();

        public GameplayEquipperService Add(IGameplayEquipper equipper)
        {
            if (equipper != null) equippers.Add(equipper);
            return this;
        }

        public bool Equip(ActorController actor, WeaponDef definition, WeaponStats stats)
        {
            foreach (var e in equippers)
                if (e.TryEquip(actor, definition, stats))
                    return true;

            UnityEngine.Debug.LogWarning($"GameplayEquipperService: No equipper handled {definition?.Class} / {definition?.Id}");
            return false;
        }
    }
}