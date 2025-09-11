using Meta.Weapons.Unlocks;

public sealed class WeaponClassUnlockRepository : IWeaponClassUnlockRepository
{
    /// <summary>
    /// File name used by your custom SaveSystem. You can pass your own via the ctor.
    /// </summary>
    public const string DefaultFileName = "weapon_class_unlock_state";

    private readonly string _fileName;

    public WeaponClassUnlockRepository(string fileName = DefaultFileName)
    {
        _fileName = string.IsNullOrWhiteSpace(fileName) ? DefaultFileName : fileName;
    }

    public WeaponClassUnlockState Load()
    {
        // Load returns defaultValue if not present; guard against null just in case.
        var state = SaveSystem.Load(_fileName, new WeaponClassUnlockState());
        return state ?? new WeaponClassUnlockState();
    }

    public void Save(WeaponClassUnlockState state)
    {
        // Never persist null; always save a valid object.
        SaveSystem.Save(_fileName, state ?? new WeaponClassUnlockState());
    }
}

