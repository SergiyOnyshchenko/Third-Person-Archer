using UI.Core;
using UnityEngine;

/// <summary>
/// Shows a one-time intro popup the first time Contracts mode becomes available
/// (company level reaches the unlock threshold).
///
/// Submits a request to StartupPopupCoordinator so the popup is shown one at a time
/// and only while the map screen is active.
///
/// Setup: Add this MonoBehaviour to the main menu scene.
/// Assign _modeUnlockConfig to the same MetaModeUnlockConfig asset used by MainMenuRuntime.
/// </summary>
public sealed class ContractsUnlockPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "mission_type_unlock_popup";
    [SerializeField] private MetaModeUnlockConfig _modeUnlockConfig;
    [SerializeField] private Sprite _icon;

    private const string SaveKey = "contracts_unlock_popup_shown";

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
            Debug.LogError("ContractsUnlockPresenter: _modeUnlockConfig is not assigned.", this);
            return;
        }

        int companyLevel = services.ProgressData.GetCompanyLevel();
        if (companyLevel < _modeUnlockConfig.ContractsUnlockCompanyLevel)
            return;

        const string title = "Contracts Unlocked!";
        const string body =
            "Repeatable missions — replay any completed Campaign level.\n" +
            "Each run earns Cash and weapon Tokens.\n" +
            "Use them to upgrade weapons and stay ahead of gates.";

        var args = new MissionTypeUnlockPopupArgs(title, body, _icon);

        if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
        {
            coord.Submit(new StartupPopupRequest
            {
                Priority     = StartupPopupPriority.ContractsUnlock,
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
