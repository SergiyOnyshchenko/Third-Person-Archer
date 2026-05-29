/// <summary>
/// Priority constants for StartupPopupCoordinator.
/// Lower number = shown first (higher urgency).
/// </summary>
public static class StartupPopupPriority
{
    /// <summary>Full-game loop transition (Zone 3 campaign complete). Shown once ever.</summary>
    public const int LoopTransition     = 10;

    /// <summary>Boss defeated in a zone — next zone or loop announcement.</summary>
    public const int BossVictory        = 20;

    /// <summary>All Campaign missions in a zone completed.</summary>
    public const int ZoneComplete       = 30;

    /// <summary>Contracts mode first unlocked (one-time feature introduction).</summary>
    public const int ContractsUnlock    = 40;

    /// <summary>Sniper mode first unlocked (one-time feature introduction).</summary>
    public const int SniperUnlock       = 50;

    /// <summary>New weapon class (Spear, Boomerang) first unlocked.</summary>
    public const int WeaponClassUnlock  = 60;

    /// <summary>Individual weapon within an already-unlocked class first unlocked.</summary>
    public const int WeaponUnlock       = 70;

    /// <summary>Repeating Contracts completion milestone (1 / 5 / 10 / 20).</summary>
    public const int ContractsMilestone = 80;
}
