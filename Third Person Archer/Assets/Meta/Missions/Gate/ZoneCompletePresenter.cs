using UI.Core;
using UnityEngine;

/// <summary>
/// Checks once per session (after MainMenuRuntime is ready) whether the player has
/// newly completed a zone's Campaign segment. Submits a CelebrationPopup request to
/// StartupPopupCoordinator the first time each zone is completed.
///
/// The last zone that has no Boss is intentionally skipped here — it is already
/// covered by LoopTransitionPresenter.
///
/// At most one zone-complete request is submitted per session (the first unclaimed zone).
///
/// Setup: Add to the main menu scene. Set _popupId to the ScreenRegistry ID of the
/// CelebrationPopupController prefab (default: "celebration_popup").
/// </summary>
public sealed class ZoneCompletePresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "celebration_popup";

    private void OnEnable()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            CheckAndSubmit(runtime.Services);
            return;
        }

        MainMenuRuntime.Ready -= OnRuntimeReady;
        MainMenuRuntime.Ready += OnRuntimeReady;
    }

    private void OnDisable()
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
    }

    private void OnRuntimeReady(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
        CheckAndSubmit(services);
    }

    private void CheckAndSubmit(MainMenuServices services)
    {
        var zones = services.ProgressData.AllZones;
        if (zones == null) return;

        for (int i = 0; i < zones.Count; i++)
        {
            var zone = zones[i];
            if (zone == null) continue;
            if (!zone.IsCampaignComplete()) continue;

            bool isLastNoBoss = !zone.HasBoss() && i == zones.Count - 1;
            if (isLastNoBoss) continue;

            string saveKey = $"zone_complete_popup_shown_{zone.ID}";
            if (SaveSystem.Load(saveKey, false)) continue;

            string zoneName = !string.IsNullOrEmpty(zone.ZoneName) ? zone.ZoneName : $"Zone {i + 1}";

            string body;
            if (zone.HasBoss())
            {
                body = $"You have cleared all Campaign missions in {zoneName}!\n\n" +
                       "The Boss is now unlocked.\n" +
                       "Upgrade your Crossbow and face it when ready.";
            }
            else
            {
                bool hasNextZone = i + 1 < zones.Count;
                string nextZoneName = hasNextZone && zones[i + 1] != null
                    ? zones[i + 1].ZoneName
                    : $"Zone {i + 2}";
                body = $"You have cleared all Campaign missions in {zoneName}!\n\n" +
                       $"Continue your journey in {nextZoneName}.";
            }

            var args = new CelebrationPopupArgs(
                title: "Zone Campaign Complete!",
                body: body
            );

            // Capture for closure — both are reference/value-type safe.
            string capturedKey = saveKey;

            if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
            {
                coord.Submit(new StartupPopupRequest
                {
                    Priority     = StartupPopupPriority.ZoneComplete,
                    LogicalId    = capturedKey,
                    PopupId      = _popupId,
                    Args         = args,
                    OnBeforeShow = () => SaveSystem.Save(capturedKey, true)
                });
            }
            else
            {
                SaveSystem.Save(capturedKey, true);
                if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                    nav.ShowPopup(_popupId, args);
            }

            break; // Submit at most one zone-complete request per session.
        }
    }
}
