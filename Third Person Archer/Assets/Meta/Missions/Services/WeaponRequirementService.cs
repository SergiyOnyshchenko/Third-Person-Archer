using Meta.Weapons;

public sealed class WeaponRequirementService : IWeaponRequirementService
{
    private readonly int _rotationStep;

    public WeaponRequirementService(int rotationStep = 1)
    {
        _rotationStep = rotationStep;
    }

    public WeaponClass GetRequiredWeaponClass(MissionData mission, int loopIndex)
    {
        if (mission == null)
            return WeaponClass.Bow;

        // Sniper (or any mission) can force fixed weapon class
        if (mission.UseFixedWeaponClass)
            return mission.FixedWeaponClass;

        // Base (loop 0)
        var baseClass = mission.BaseWeaponClass;

        // If rotation disabled, always use base
        if (!mission.AllowLoopRotation)
            return baseClass;

        // Rotation (Option B)
        int classCount = System.Enum.GetValues(typeof(WeaponClass)).Length;
        int baseIndex = (int)baseClass;

        int rotated = baseIndex + (loopIndex * _rotationStep);
        rotated = Mod(rotated, classCount);

        return (WeaponClass)rotated;
    }

    private static int Mod(int value, int m)
    {
        int r = value % m;
        return r < 0 ? r + m : r;
    }
}