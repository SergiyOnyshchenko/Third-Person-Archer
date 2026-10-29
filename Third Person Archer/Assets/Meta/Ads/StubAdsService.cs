using System;
using UnityEngine;

namespace Meta.Ads
{
    /// <summary>
    /// Placeholder until an ads SDK is integrated: the ad is "watched" instantly.
    /// Toggle <see cref="SimulateFailure"/> / <see cref="SimulateNotReady"/> to test fail paths.
    /// </summary>
    public sealed class StubAdsService : IAdsService
    {
        public static bool SimulateFailure;
        public static bool SimulateNotReady;

        public bool IsRewardedReady(string placementId) => !SimulateNotReady;

        public void ShowRewarded(string placementId, Action<bool> onComplete)
        {
            bool success = !SimulateNotReady && !SimulateFailure;
            Debug.Log($"[StubAds] Rewarded '{placementId}' -> {(success ? "rewarded" : "failed")}");
            onComplete?.Invoke(success);
        }
    }
}
