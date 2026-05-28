public interface IMissionGateService
{
    MissionGateResult CheckCampaignGate(MissionContext ctx);

    /// <summary>
    /// Checks whether the player's equipped Crossbow damage meets the access threshold
    /// for the given Sniper tier (sniperCompletedIndex = number of Sniper missions completed so far).
    /// Does NOT affect enemy HP — purely an access gate.
    /// </summary>
    MissionGateResult CheckSniperGate(MissionContext ctx, int sniperCompletedIndex);

    /// <summary>
    /// Checks whether the player's equipped Crossbow damage meets the access threshold
    /// for the Boss mission in the given zone (ctx.ZoneIndex = 0-based zone index).
    /// Does NOT affect boss HP/damage gameplay scaling — purely a meta progression gate.
    /// </summary>
    MissionGateResult CheckBossGate(MissionContext ctx);
}
