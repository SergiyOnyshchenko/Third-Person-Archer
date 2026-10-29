using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup shown when the next Campaign/Boss gate is blocked and the player cannot
/// currently upgrade or buy — they need to grind resources first.
///
/// Action button → directly replays the last cleared level of the class (gold + its essence).
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

        if (_titleText != null)         _titleText.text = $"Need more {args.WeaponClass} essence";
        if (_bodyText != null)          _bodyText.text  = $"Replay a {args.WeaponClass} level you cleared: gold and essence for the upgrade.";
        if (_actionButtonLabel != null) _actionButtonLabel.text = "Replay";
    }

    private void OnActionClicked()
    {
        if (_args == null) { Close(); return; }

        var services = MainMenuRuntime.Instance?.Services;
        if (services == null) { Close(); return; }

        var result = services.MissionStart.TryStartReplay(_args.WeaponClass);

        // Always close popup — either the replay loads or the player picks a level on the map.
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
