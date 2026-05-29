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
/// Set _popupId = "celebration_popup" and _contractsUnlockCompanyLevel to match
/// the value set in MetaModeUnlockConfig (default: 5).
/// </summary>
public sealed class ContractsUnlockPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "celebration_popup";

    [Tooltip("Must match MetaModeUnlockConfig.ContractsUnlockCompanyLevel.")]
    [SerializeField] private int _contractsUnlockCompanyLevel = 5;

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

        int companyLevel = services.ProgressData.GetCompanyLevel();
        if (companyLevel < _contractsUnlockCompanyLevel)
            return;

        const string body =
            "Contracts are repeatable grind missions.\n\n" +
            "Run any Campaign mission you have already completed. " +
            "Every Contract run rewards tokens for ALL weapon classes — not just one.\n\n" +
            "Rewards: Cash and tokens for every weapon class.\n\n" +
            "Use Contracts regularly to keep your weapons upgraded and stay ahead of campaign gates.";

        var args = new CelebrationPopupArgs(
            title: "Contracts Unlocked!",
            body: body
        );

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
