using Meta.Weapons;

public sealed class LoadoutSnapshotWeaponStatService : ILoadoutWeaponStatService
{
    private readonly LoadoutSnapshot _snapshot;

    public LoadoutSnapshotWeaponStatService(LoadoutSnapshot snapshot)
    {
        _snapshot = snapshot;
    }

    public float GetEquippedDamage(WeaponClass weaponClass)
    {
        if (_snapshot == null)
            return 0f;

        if (_snapshot.TryGet(weaponClass, out var slot) && slot != null)
            return slot.Damage;

        return 0f;
    }
}