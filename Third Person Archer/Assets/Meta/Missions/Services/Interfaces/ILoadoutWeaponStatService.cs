using Meta.Weapons;

public interface ILoadoutWeaponStatService
{
    float GetEquippedDamage(WeaponClass weaponClass);

    /// <summary>Returns the maximum possible damage of the currently equipped weapon at max upgrade level.</summary>
    float GetEquippedMaxDamage(WeaponClass weaponClass);
}
