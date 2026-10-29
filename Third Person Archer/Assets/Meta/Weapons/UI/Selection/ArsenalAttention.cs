using System;
using UI.Core;

namespace Meta.Weapons.UI
{
    /// <summary>Red-dot rule of the arsenal: an upgrade is affordable or the next tier can be bought.</summary>
    public static class ArsenalAttention
    {
        public static int CompanyLevel =>
            ServiceLocator.TryResolve<IWeaponClassUnlockService>(out var unlocks) && unlocks != null
                ? unlocks.CurrentCompanyLevel
                : 1;

        public static bool IsClassUnlocked(WeaponClass cls) =>
            !ServiceLocator.TryResolve<IWeaponClassUnlockService>(out var unlocks) || unlocks == null ||
            unlocks.IsClassUnlocked(cls);

        public static bool HasSomethingToBuy(WeaponClass cls)
        {
            var weapons = WeaponsInitializer.Instance;
            if (weapons == null || weapons.Progression == null || weapons.UpgradeService == null) return false;

            if (weapons.UpgradeService.CanAffordUpgrade(cls)) return true;

            var next = weapons.Progression.GetNextTierDef(cls);
            return next != null && weapons.Progression.GetTierPurchaseBlock(next, CompanyLevel) == TierPurchaseBlock.None;
        }

        public static bool AnyClassHasSomethingToBuy()
        {
            foreach (WeaponClass cls in Enum.GetValues(typeof(WeaponClass)))
                if (IsClassUnlocked(cls) && HasSomethingToBuy(cls))
                    return true;

            return false;
        }
    }
}
