// EquipmentService.cs
using System.Linq;

namespace Meta.Weapons
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IWeaponRepository _repository;

        public event System.Action<WeaponClass, string> OnEquippedWeaponChanged;

        public EquipmentService(IWeaponRepository repository)
        {
            _repository = repository;
        }

        public bool Equip(WeaponClass cls, string weaponId)
        {
            var state = _repository.Load();

            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId && w.Owned);
            if (inst == null) return false;

            var defClass = cls;
            // Optional: you can validate that weapon’s real class matches the slot's expected class
            // via WeaponCatalog if you inject it; for now we trust the caller.

            SetEquippedId(ref state, defClass, weaponId);
            _repository.Save(state);

            OnEquippedWeaponChanged?.Invoke(defClass, weaponId);
            return true;
        }

        public string GetEquippedWeaponId(WeaponsState state, WeaponClass cls)
        {
            return cls switch
            {
                WeaponClass.Bow       => state.EquippedBowId,
                WeaponClass.Crossbow  => state.EquippedCrossbowId,
                WeaponClass.Spear     => state.EquippedSpearId,
                WeaponClass.Shuriken  => state.EquippedShurikenId,
                WeaponClass.Boomerang => state.EquippedBoomerangId,
                _ => null
            };
        }

        public string GetEquippedWeaponId(WeaponClass cls)
        {
            var state = _repository.Load();
            return GetEquippedWeaponId(state, cls);
        }

        private static void SetEquippedId(ref WeaponsState state, WeaponClass cls, string weaponId)
        {
            switch (cls)
            {
                case WeaponClass.Bow:
                    state.EquippedBowId = weaponId;
                    break;
                case WeaponClass.Crossbow:
                    state.EquippedCrossbowId = weaponId;
                    break;
                case WeaponClass.Spear:
                    state.EquippedSpearId = weaponId;
                    break;
                case WeaponClass.Shuriken:
                    state.EquippedShurikenId = weaponId;
                    break;
                case WeaponClass.Boomerang:
                    state.EquippedBoomerangId = weaponId;
                    break;
            }
        }
    }
}