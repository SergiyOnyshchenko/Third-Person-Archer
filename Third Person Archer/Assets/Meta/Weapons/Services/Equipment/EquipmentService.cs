using System.Linq;

namespace Meta.Weapons
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IWeaponRepository repository;

        public event System.Action<string> OnEquippedWeaponChanged; // weaponId (or null if none)

        public EquipmentService(IWeaponRepository repository)
        {
            this.repository = repository;
        }

        public bool Equip(string weaponId)
        {
            var state = repository.Load();
            var target = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId && w.Owned);
            if (target == null) return false;

            // Unequip EVERYTHING (global exclusive)
            foreach (var w in state.Weapons) w.Equipped = false;

            target.Equipped = true;
            repository.Save(state);
            OnEquippedWeaponChanged?.Invoke(weaponId);
            return true;
        }

        public bool Unequip(string weaponId)
        {
            var state = repository.Load();
            var target = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId);
            if (target == null || !target.Equipped) return false;

            target.Equipped = false;
            repository.Save(state);
            OnEquippedWeaponChanged?.Invoke(null);
            return true;
        }

        public string GetEquippedWeaponId(WeaponsState state)
        {
            return state.Weapons.FirstOrDefault(w => w.Equipped)?.WeaponId;
        }
    }
}
