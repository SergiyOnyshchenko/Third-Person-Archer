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

        bool passed = current >= required;
        return new MissionGateResult(passed, required, current, ctx.RequiredWeaponClass);
    }
}