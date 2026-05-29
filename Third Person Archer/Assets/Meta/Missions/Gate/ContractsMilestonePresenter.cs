using UI.Core;
using UnityEngine;

/// <summary>
/// Checks once per session whether the player has reached a new Contracts completion
/// milestone. Submits a CelebrationPopup request to StartupPopupCoordinator the first
/// time each threshold is crossed.
///
/// Milestones: 1, 5, 10, 20 completed Contracts.
/// Only the highest unclaimed milestone is shown per session.
///
/// No currency rewards are granted here — this is UI feedback only.
///
/// Setup: Add to the main menu scene. Set _popupId to the ScreenRegistry ID of the
/// CelebrationPopupController prefab (default: "celebration_popup").
/// </summary>
public sealed class ContractsMilestonePresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "celebration_popup";

    private static readonly int[] Milestones = { 1, 5, 10, 20 };
    private const string SaveKey = "contracts_milestone_highest_shown";

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
        int completed = services.LoopData.ContractsCompletedIndex;
        int highestShown = SaveSystem.Load(SaveKey, 0);

        int targetMilestone = -1;
        foreach (int m in Milestones)
        {
            if (completed >= m && highestShown < m)
                targetMilestone = m;
        }

        if (targetMilestone < 0)
            return;

        string title = targetMilestone == 1
            ? "First Contract Complete!"
            : $"Contract Milestone: {targetMilestone} Completed!";

        string body = targetMilestone == 1
            ? "You completed your first Contract!\n\n" +
              "Contracts are the best way to earn tokens for all weapon classes.\n" +
              "Run them regularly to keep your upgrades moving."
            : $"You have completed {targetMilestone} Contracts!\n\n" +
              "Keep running Contracts to earn tokens and unlock stronger weapons.\n" +
              "Every run counts toward your next upgrade.";

        var args = new CelebrationPopupArgs(title: title, body: body);

        // Capture for closure — targetMilestone is a value type so it is safely captured.
        int capturedMilestone = targetMilestone;

        if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
        {
            coord.Submit(new StartupPopupRequest
            {
                Priority     = StartupPopupPriority.ContractsMilestone,
                LogicalId    = $"contracts_milestone_{capturedMilestone}",
                PopupId      = _popupId,
                Args         = args,
                OnBeforeShow = () => SaveSystem.Save(SaveKey, capturedMilestone)
            });
        }
        else
        {
            SaveSystem.Save(SaveKey, capturedMilestone);
            if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                nav.ShowPopup(_popupId, args);
        }
    }
}
