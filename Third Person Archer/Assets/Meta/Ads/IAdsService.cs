using System;

/// <summary>
/// Rewarded ads abstraction. A real SDK adapter (LevelPlay, etc.) implements this
/// and is assigned to <see cref="Meta.Ads.Ads.Service"/> at bootstrap.
/// </summary>
public interface IAdsService
{
    bool IsRewardedReady(string placementId);

    /// <summary>onComplete(true) only when the reward was earned.</summary>
    void ShowRewarded(string placementId, Action<bool> onComplete);
}
