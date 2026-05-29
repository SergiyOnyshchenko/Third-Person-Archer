using UI.Core;
using UnityEngine;

/// <summary>
/// Checks once per session (after MainMenuRuntime is ready) whether the player has
/// newly defeated a Boss. Submits a CelebrationPopup request to
/// StartupPopupCoordinator the first time each zone boss is defeated.
///
/// At most one boss-victory request is submitted per session (the first unshown zone).
///
/// Setup: Add to the main menu scene. Set _popupId to the ScreenRegistry ID of the
/// CelebrationPopupController prefab (default: "celebration_popup").
/// </summary>
public sealed class BossVictoryPresenter : MonoBehaviour
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
            if (zone == null || !zone.HasBoss()) continue;
            if (!zone.IsBossCompleted()) continue;

            string saveKey = $"boss_victory_popup_shown_{zone.ID}";
            if (SaveSystem.Load(saveKey, false)) continue;

            string zoneName = !string.IsNullOrEmpty(zone.ZoneName) ? zone.ZoneName : $"Zone {i + 1}";

            string nextStep;
            int nextZoneIndex = i + 1;
            if (nextZoneIndex < zones.Count)
            {
                var nextZone = zones[nextZoneIndex];
                string nextZoneName = nextZone != null && !string.IsNullOrEmpty(nextZone.ZoneName)
                    ? nextZone.ZoneName
                    : $"Zone {nextZoneIndex + 1}";
                nextStep = $"{nextZoneName} is now open!\n\nKeep fighting and push through the next campaign.";
            }
            else
            {
                nextStep = "You have conquered all Bosses!\n\nA new challenge loop begins — stronger gates, better rewards. Your weapons and progress carry over.";
            }

            string body = $"You have defeated the Boss of {zoneName}!\n\n{nextStep}";

            var args = new CelebrationPopupArgs(
                title: "Boss Defeated!",
                body: body
            );

            string capturedKey = saveKey;

            if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
            {
                coord.Submit(new StartupPopupRequest
                {
                    Priority     = StartupPopupPriority.BossVictory,
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

            break; // Submit at most one boss-victory request per session.
        }
    }
}
