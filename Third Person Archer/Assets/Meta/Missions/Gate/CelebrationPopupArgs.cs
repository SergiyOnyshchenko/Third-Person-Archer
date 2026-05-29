/// <summary>
/// Args passed to CelebrationPopupController via UINavigator.ShowPopup.
/// Used for zone complete, boss victory, and contracts milestone popups.
/// </summary>
public sealed class CelebrationPopupArgs
{
    public string Title { get; }
    public string Body { get; }

    public CelebrationPopupArgs(string title, string body)
    {
        Title = title;
        Body = body;
    }
}
