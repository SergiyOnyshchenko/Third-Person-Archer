using UI.Core;
using UnityEngine;

/// <summary>
/// Checks once per session (after MainMenuRuntime is ready) whether the player has
/// completed the final zone's Campaign (which has no Boss). If so, and if the popup
/// has not been shown before, submits a LoopTransitionPopup request to
/// StartupPopupCoordinator.
///
/// The popup is shown only when the map screen is active (coordinator rule).
/// The one-time save flag is written immediately before the popup is displayed.
///
/// Setup: Add this MonoBehaviour to the main menu scene. Set _popupId to match the
/// ScreenRegistry entry for LoopTransitionPopupController.
/// </summary>
public sealed class LoopTransitionPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "loop_transition_popup";

    private const string SaveKey = "loop_transition_shown";

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
        if (SaveSystem.Load(SaveKey, false))
            return;

        var zones = services.ProgressData.AllZones;
        if (zones == null || zones.Count == 0)
            return;

        var lastZone = zones[zones.Count - 1];
        if (lastZone == null || lastZone.HasBoss())
            return;

        if (!lastZone.IsCampaignComplete())
            return;

        for (int i = 0; i < zones.Count - 1; i++)
        {
            var zone = zones[i];
            if (zone == null) continue;
            bool complete = zone.HasBoss() ? zone.IsBossCompleted() : zone.IsCampaignComplete();
            if (!complete) return;
        }

        // Submit to coordinator — save flag is written by OnBeforeShow (just before display).
        if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
        {
            coord.Submit(new StartupPopupRequest
            {
                Priority    = StartupPopupPriority.LoopTransition,
                LogicalId   = SaveKey,
                PopupId     = _popupId,
                Args        = null,
                OnBeforeShow = () => SaveSystem.Save(SaveKey, true)
            });
        }
        else
        {
            // Fallback: no coordinator in scene — show directly (original behaviour).
            SaveSystem.Save(SaveKey, true);
            if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                nav.ShowPopup(_popupId, null);
        }
    }
}
