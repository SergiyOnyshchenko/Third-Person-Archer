using Meta.Weapons;

public sealed class MissionGateService : IMissionGateService
{
    private readonly BalanceConfig _balance;
    private readonly ILoadoutWeaponStatService _loadoutStats;

    public MissionGateService(BalanceConfig balance, ILoadoutWeaponStatService loadoutStats)
    {
        _balance = balance;
        _loadoutStats = loadoutStats;
    }

    public MissionGateResult CheckCampaignGate(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionGateResult(false, 0, 0, WeaponClass.Bow);

        // Gate applies ONLY to Campaign missions.
        if (ctx.SelectedType != MissionType.Campaign)
            return new MissionGateResult(true, 0, 0, ctx.RequiredWeaponClass);

        // Soft gate: playable from min damage (Yellow), full power at recommended R (Green).
        float recommended = _balance.GetCampaignRecommendedDamage(ctx);
        float required = _balance.GetCampaignMinDamage(ctx);

        float current = _loadoutStats.GetEquippedDamage(ctx.RequiredWeaponClass);
        float maxDamage = _loadoutStats.GetEquippedMaxDamage(ctx.RequiredWeaponClass);

        var status = current >= recommended ? MissionGateStatus.Green
                   : current >= required ? MissionGateStatus.Yellow
                   : MissionGateStatus.Red;

        bool canUpgrade = maxDamage >= required;
        return new MissionGateResult(status, required, recommended, current, ctx.RequiredWeaponClass, canUpgrade);
    }

    public MissionGateResult CheckBossGate(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionGateResult(false, 0, 0, WeaponClass.Crossbow);

        // Gate applies ONLY to Boss missions.
        if (ctx.SelectedType != MissionType.Boss)
            return new MissionGateResult(true, 0, 0, WeaponClass.Crossbow);

        float required = _balance.GetBossGateDamage(ctx);
        float current = _loadoutStats.GetEquippedDamage(WeaponClass.Crossbow);
        float maxDamage = _loadoutStats.GetEquippedMaxDamage(WeaponClass.Crossbow);

        bool passed = current >= required;
        bool canUpgrade = maxDamage >= required;
        return new MissionGateResult(passed, required, current, WeaponClass.Crossbow, canUpgrade);
    }
}
