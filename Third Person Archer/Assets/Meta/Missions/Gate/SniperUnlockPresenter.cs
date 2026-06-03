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
/// Assign _modeUnlockConfig to the same MetaModeUnlockConfig asset used by MainMenuRuntime.
/// </summary>
public sealed class SniperUnlockPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "mission_type_unlock_popup";
    [SerializeField] private MetaModeUnlockConfig _modeUnlockConfig;
    [SerializeField] private Sprite _icon;

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

        if (_modeUnlockConfig == null)
        {
            Debug.LogError("SniperUnlockPresenter: _modeUnlockConfig is not assigned.", this);
            return;
        }

        int companyLevel = services.ProgressData.GetCompanyLevel();
        if (companyLevel < _modeUnlockConfig.SniperUnlockCompanyLevel)
            return;

        const string title = "Sniper Unlocked!";
        const string body =
            "Precision missions using the Crossbow.\n" +
            "One shot per enemy — aim carefully.\n" +
            "Rewards Cash and Crossbow Tokens.\n" +
            "Upgrade your Crossbow to unlock Sniper tiers and pass Boss gates.";

        var args = new MissionTypeUnlockPopupArgs(title, body, _icon);

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
