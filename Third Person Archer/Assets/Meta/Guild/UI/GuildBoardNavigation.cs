using UI.Core;

/// <summary>Contracts are played from the Guild Board screen instead of a direct start.</summary>
public static class GuildBoardNavigation
{
    public const string ScreenId = "guild_board";

    public static bool Open()
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return false;

        nav.Open(ScreenId);
        return true;
    }
}
