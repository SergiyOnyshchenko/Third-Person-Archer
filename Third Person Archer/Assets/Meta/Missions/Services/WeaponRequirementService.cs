using Meta.Weapons;

public sealed class WeaponRequirementService : IWeaponRequirementService
{
    public WeaponClass GetRequiredWeaponClass(MissionData mission)
    {
        if (mission == null)
            return WeaponClass.Bow;

        // Sniper (or any mission) can force fixed weapon class
        return mission.UseFixedWeaponClass ? mission.FixedWeaponClass : mission.BaseWeaponClass;
    }
}
