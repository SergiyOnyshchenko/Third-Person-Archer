namespace Meta.Weapons
{
    public interface IStatsService
    {
        /// <summary>
        /// Computes final stats for the given weapon instance using its definition, part levels, and mastery tiers.
        /// </summary>
        WeaponStats Compute(WeaponDef def, WeaponInstance instance);
    }
}