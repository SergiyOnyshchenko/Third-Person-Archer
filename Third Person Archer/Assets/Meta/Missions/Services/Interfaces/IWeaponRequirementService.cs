using Meta.Weapons;

public interface IWeaponRequirementService
{
    WeaponClass GetRequiredWeaponClass(MissionData mission, int loopIndex);
}