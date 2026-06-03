using UnityEngine;

/// <summary>
/// Args passed to MissionTypeUnlockPopupController via UINavigator.ShowPopup.
/// Used for: Contracts unlock, Sniper unlock.
/// </summary>
public sealed class MissionTypeUnlockPopupArgs
{
    public string Title { get; }
    public string Body  { get; }
    public Sprite Icon  { get; }

    public MissionTypeUnlockPopupArgs(string title, string body, Sprite icon = null)
    {
        Title = title;
        Body  = body;
        Icon  = icon;
    }
}
