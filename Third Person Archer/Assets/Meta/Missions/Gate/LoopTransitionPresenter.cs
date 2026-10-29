using UI.Core;
using UnityEngine;

/// <summary>
/// Campaign finale (D11): once the whole campaign is complete (last zone has no Boss), shows the
/// "Heroic mode unlocked" popup once via StartupPopupCoordinator (top priority).
/// Class name kept from the old loop-transition popup (scene/registry references).
/// </summary>
public sealed class LoopTransitionPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "loop_transition_popup";

    private const string SaveKey = "heroic_unlock_shown";

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

        if (services.ProgressData == null || !services.ProgressData.IsHeroicUnlocked())
            return;

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
