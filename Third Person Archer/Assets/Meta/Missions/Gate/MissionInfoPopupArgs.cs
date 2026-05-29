using Meta.Economy;

/// <summary>
/// Args passed to MissionInfoPopupController via UINavigator.ShowPopup.
/// Describes a mission type: what it is, what currencies it rewards, and a short reward note.
/// </summary>
public sealed class MissionInfoPopupArgs
{
    /// <summary>Mission type name shown as the popup title.</summary>
    public string Title { get; }

    /// <summary>One or two sentence description of the mode. Keep it short.</summary>
    public string Description { get; }

    /// <summary>
    /// Currency types to display as reward icons.
    /// The popup shows one icon per entry (using CurrencyVisualLibrary for sprite + label).
    /// </summary>
    public CurrencyType[] RewardTypes { get; }

    /// <summary>
    /// Short one-line note placed below the reward icons.
    /// Example: "Token type depends on the mission's weapon class"
    /// </summary>
    public string RewardNote { get; }

    public MissionInfoPopupArgs(
        string title,
        string description,
        CurrencyType[] rewardTypes,
        string rewardNote = null)
    {
        Title = title;
        Description = description;
        RewardTypes = rewardTypes ?? System.Array.Empty<CurrencyType>();
        RewardNote = rewardNote;
    }
}
