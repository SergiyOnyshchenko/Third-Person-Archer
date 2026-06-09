/// <summary>
/// Args for NotEnoughResourcesPopupController.
/// Shown when the next gate is blocked and the player has no affordable upgrade/buy
/// option — they must grind resources first.
/// </summary>
public sealed class NotEnoughResourcesPopupArgs
{
    /// <summary>
    /// True  → Crossbow gate (Boss or Sniper-access): direct-start a Sniper mission.
    /// False → normal weapon gate: direct-start a Contracts mission.
    /// </summary>
    public bool IsCrossbowGate { get; }

    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    public NotEnoughResourcesPopupArgs(bool isCrossbowGate, float currentDamage, float requiredDamage)
    {
        IsCrossbowGate = isCrossbowGate;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
    }
}
