namespace Meta.Weapons
{
    public interface IWeaponRepository
    {
        WeaponsState Load();
        void Save(WeaponsState state);
    }
}
