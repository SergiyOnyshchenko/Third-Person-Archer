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

        float required = _balance.GetRequiredDamageForCampaign(
            ctx.RequiredWeaponClass,
            ctx.GlobalCampaignIndex,
            ctx.BalanceLoopIndex);

        float current = _loadoutStats.GetEquippedDamage(ctx.RequiredWeaponClass);
        float maxDamage = _loadoutStats.GetEquippedMaxDamage(ctx.RequiredWeaponClass);

        bool passed = current >= required;
        bool canUpgrade = maxDamage >= required;
        return new MissionGateResult(passed, required, current, ctx.RequiredWeaponClass, canUpgrade);
    }

    public MissionGateResult CheckSniperGate(MissionContext ctx, int sniperCompletedIndex)
    {
        if (ctx == null)
            return new MissionGateResult(false, 0, 0, WeaponClass.Crossbow);

        // Gate applies ONLY to Sniper missions.
        if (ctx.SelectedType != MissionType.Sniper)
            return new MissionGateResult(true, 0, 0, WeaponClass.Crossbow);

        float required = _balance.GetRequiredCrossbowDamageForSniper(sniperCompletedIndex, ctx.BalanceLoopIndex);
        float current = _loadoutStats.GetEquippedDamage(WeaponClass.Crossbow);
        float maxDamage = _loadoutStats.GetEquippedMaxDamage(WeaponClass.Crossbow);

        bool passed = current >= required;
        bool canUpgrade = maxDamage >= required;
        return new MissionGateResult(passed, required, current, WeaponClass.Crossbow, canUpgrade);
    }

    public MissionGateResult CheckBossGate(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionGateResult(false, 0, 0, WeaponClass.Crossbow);

        // Gate applies ONLY to Boss missions.
        if (ctx.SelectedType != MissionType.Boss)
            return new MissionGateResult(true, 0, 0, WeaponClass.Crossbow);

        float required = _balance.GetRequiredCrossbowDamageForBoss(ctx.ZoneIndex, ctx.BalanceLoopIndex);
        float current = _loadoutStats.GetEquippedDamage(WeaponClass.Crossbow);
        float maxDamage = _loadoutStats.GetEquippedMaxDamage(WeaponClass.Crossbow);

        bool passed = current >= required;
        bool canUpgrade = maxDamage >= required;
        return new MissionGateResult(passed, required, current, WeaponClass.Crossbow, canUpgrade);
    }
}
