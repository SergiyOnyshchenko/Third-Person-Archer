using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup shown when the next Campaign/Boss gate is blocked and the player cannot
/// currently upgrade or buy — they need to grind resources first.
///
/// Action button → directly starts a Contracts or Sniper mission (direct-start path).
///   1. Selects the appropriate mission type via Progress.SelectMissionType().
///   2. Calls MissionStart.TryStartSelected().
///   3. If successful, closes popup and loads the mission scene.
///   4. If start fails (no eligible missions, etc.), closes popup with the correct
///      tab already selected so the player can press Play manually.
///
/// Later button → closes popup with no action.
///
/// Setup: create prefab, assign all [SerializeField] refs, register in ScreenRegistry
/// under the ID configured in PostMissionNextStepPresenter (default: "not_enough_resources_popup").
/// </summary>
public sealed class NotEnoughResourcesPopupController : MonoBehaviour, IReceivesArgs<NotEnoughResourcesPopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bodyText;

    [Header("Buttons")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonLabel;
    [SerializeField] private Button _laterButton;

    private NotEnoughResourcesPopupArgs _args;

    private void Awake()
    {
        if (_actionButton != null) _actionButton.onClick.AddListener(OnActionClicked);
        if (_laterButton  != null) _laterButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (_actionButton != null) _actionButton.onClick.RemoveListener(OnActionClicked);
        if (_laterButton  != null) _laterButton.onClick.RemoveListener(Close);
    }

    public bool ValidateArgs(NotEnoughResourcesPopupArgs args) => args != null;

    public void ApplyArgs(NotEnoughResourcesPopupArgs args)
    {
        _args = args;

        if (args.IsCrossbowGate)
        {
            if (_titleText != null)       _titleText.text = "Need Crossbow Tokens";
            if (_bodyText != null)        _bodyText.text  = "Play Sniper missions to upgrade your Crossbow.";
            if (_actionButtonLabel != null) _actionButtonLabel.text = "Play Sniper";
        }
        else
        {
            if (_titleText != null)       _titleText.text = "Not enough resources";
            if (_bodyText != null)        _bodyText.text  = "Play Contracts to earn cash and weapon tokens.";
            if (_actionButtonLabel != null) _actionButtonLabel.text = "Play Contracts";
        }
    }

    private void OnActionClicked()
    {
        if (_args == null) { Close(); return; }

        var services = MainMenuRuntime.Instance?.Services;
        if (services == null) { Close(); return; }

        var targetType = _args.IsCrossbowGate ? MissionType.Sniper : MissionType.Contracts;

        // Switch to the correct tab — required for BuildSelectedContext to return the right type.
        // Side effect: OnMissionTypeChanged fires, which refreshes all map presenters (harmless).
        services.Progress.SelectMissionType(targetType);

        // Attempt direct start using the existing play flow.
        var result = services.MissionStart.TryStartSelected();

        // Always close popup — either mission scene loads or the correct tab is already selected.
        Destroy(gameObject);

        if (result.Started && result.MissionToLoad?.Scene != null &&
            !string.IsNullOrEmpty(result.MissionToLoad.Scene.ScenePath))
        {
            ScenesLoader.Instance?.LoadScene(result.MissionToLoad.Scene.ScenePath);
        }
        // Fallback (start failed): popup closed, map shows correct tab, player presses Play manually.
    }

    private void Close() => Destroy(gameObject);
}
