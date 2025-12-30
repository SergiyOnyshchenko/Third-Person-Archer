namespace Meta.Weapons
{
    public interface IWeaponClassUnlockService
    {
        bool IsClassUnlocked(WeaponClass weaponClass);
        int LastUnlockedZoneIndex { get; }
        int CurrentCompanyLevel { get; }
    }
}
