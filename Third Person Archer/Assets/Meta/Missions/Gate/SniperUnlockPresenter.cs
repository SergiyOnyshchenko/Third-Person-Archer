using UI.Core;
using UnityEngine;

/// <summary>
/// Shows a one-time intro popup the first time Sniper mode becomes available
/// (company level reaches the unlock threshold).
///
/// Submits a request to StartupPopupCoordinator so the popup is shown one at a time
/// and only while the map screen is active.
///
/// Setup: Add this MonoBehaviour to the main menu scene.
/// Set _popupId = "celebration_popup" and _sniperUnlockCompanyLevel to match
/// the value set in MetaModeUnlockConfig (default: 5).
/// </summary>
public sealed class SniperUnlockPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "celebration_popup";

    [Tooltip("Must match MetaModeUnlockConfig.SniperUnlockCompanyLevel.")]
    [SerializeField] private int _sniperUnlockCompanyLevel = 5;

    private const string SaveKey = "sniper_unlock_popup_shown";

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

        int companyLevel = services.ProgressData.GetCompanyLevel();
        if (companyLevel < _sniperUnlockCompanyLevel)
            return;

        const string body =
            "Sniper missions use the Crossbow — one precise shot to eliminate each target.\n\n" +
            "The Sniper gameplay is different from Campaign: take your time, aim carefully, " +
            "and make every shot count.\n\n" +
            "Rewards: Cash and Crossbow Tokens.\n\n" +
            "Crossbow Tokens are essential for upgrading your Crossbow — which you will need to " +
            "unlock access to Sniper tiers and pass Boss Crossbow gates.";

        var args = new CelebrationPopupArgs(
            title: "Sniper Unlocked!",
            body: body
        );

        if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
        {
            coord.Submit(new StartupPopupRequest
            {
                Priority     = StartupPopupPriority.SniperUnlock,
                LogicalId    = SaveKey,
                PopupId      = _popupId,
                Args         = args,
                OnBeforeShow = () => SaveSystem.Save(SaveKey, true)
            });
        }
        else
        {
            SaveSystem.Save(SaveKey, true);
            if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                nav.ShowPopup(_popupId, args);
        }
    }
}
