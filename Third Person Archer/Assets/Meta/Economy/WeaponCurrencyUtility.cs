using Meta.Economy;

namespace Meta.Weapons
{
    public static class WeaponCurrencyUtility
    {
        public static CurrencyType GetTokenCurrency(WeaponClass weaponClass)
        {
            switch (weaponClass)
            {
                case WeaponClass.Bow:
                    return CurrencyType.BowToken;
                case WeaponClass.Crossbow:
                    return CurrencyType.CrossbowToken;
                case WeaponClass.Spear:
                    return CurrencyType.SpearToken;
                case WeaponClass.Shuriken:
                    return CurrencyType.ShurikenToken;
                case WeaponClass.Boomerang:
                    return CurrencyType.BoomerangToken;
                default:
                    // Fallback if new WeaponClass is added and not mapped yet.
                    return CurrencyType.Gold; // or throw, or use generic token
            }
        }
    }
}