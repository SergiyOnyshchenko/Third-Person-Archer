using Meta.Weapons;
using Meta.Weapons.UI;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class DamageGatePopupController : MonoBehaviour, IReceivesArgs<DamageGatePopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _currentDamageText;
    [SerializeField] private TextMeshProUGUI _requiredDamageText;
    [SerializeField] private TextMeshProUGUI _hintText;

    [Header("Buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _goButton;
    [SerializeField] private TextMeshProUGUI _goButtonLabel;

    [Header("Tutorial (unused since Phase 4 — hide in prefab)")]
    [SerializeField] private GameObject _tutorialRoot;
    [SerializeField] private TextMeshProUGUI _tutorialBodyText;

    private DamageGatePopupArgs _args;

    private void Awake()
    {
        if (_closeButton != null) _closeButton.onClick.AddListener(Close);
        if (_goButton    != null) _goButton.onClick.AddListener(OnGoButtonClicked);
    }

    private void OnDestroy()
    {
        if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
        if (_goButton    != null) _goButton.onClick.RemoveListener(OnGoButtonClicked);
    }

    public bool ValidateArgs(DamageGatePopupArgs args)
    {
        return args != null && !string.IsNullOrEmpty(args.WeaponScreenId);
    }

    public void ApplyArgs(DamageGatePopupArgs args)
    {
        _args = args;

        // Title
        if (_titleText != null)
            _titleText.text = BuildTitle(args);

        // Damage numbers
        if (_currentDamageText != null)
            _currentDamageText.text = $"{args.CurrentDamage:0.##}";
        if (_requiredDamageText != null)
            _requiredDamageText.text = $"{args.RequiredDamage:0.##}";

        // Short, action-matched hint
        if (_hintText != null)
            _hintText.text = BuildHint(args.Action);

        // Action button label
        if (_goButtonLabel != null)
            _goButtonLabel.text = BuildButtonLabel(args.Action);

        // Tutorial section is always hidden in Phase 4+
        if (_tutorialRoot != null)
            _tutorialRoot.SetActive(false);
    }

    // ── Text helpers ──────────────────────────────────────────────────────────

    private static string BuildTitle(DamageGatePopupArgs args)
    {
        // Crossbow (Boss / Sniper access) gets a dedicated title.
        if (args.WeaponClass == WeaponClass.Crossbow)
            return "Your Crossbow is too weak";

        return $"Your {args.WeaponClass} is too weak";
    }

    private static string BuildHint(GatePopupAction action)
    {
        return action switch
        {
            GatePopupAction.PlayContracts => "Play Contracts to earn cash and weapon tokens.",
            GatePopupAction.PlaySniper    => "Play Sniper missions to earn Crossbow Tokens.",
            _                             => "Upgrade it or buy a stronger one to continue.",
        };
    }

    private static string BuildButtonLabel(GatePopupAction action)
    {
        return action switch
        {
            GatePopupAction.PlayContracts => "Play Contracts",
            GatePopupAction.PlaySniper    => "Play Sniper",
            _                             => "Open Weapons",
        };
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void Close() => Destroy(gameObject);

    private void OnGoButtonClicked()
    {
        if (_args == null) { Close(); return; }

        switch (_args.Action)
        {
            case GatePopupAction.PlayContracts:
                StartMissionThenClose(MissionType.Contracts);
                break;
            case GatePopupAction.PlaySniper:
                StartMissionThenClose(MissionType.Sniper);
                break;
            default:
                OpenWeapons();
                break;
        }
    }

    private void OpenWeapons()
    {
        Destroy(gameObject);

        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

        var weaponArgs = new WeaponSelectionArgs(
            _args.WeaponClass,
            _args.CampaignLevel,
            _args.PreselectWeaponId,
            _args.HighlightMode);

        nav.Open(_args.WeaponScreenId, weaponArgs, reuseCached: true);
    }

    private void StartMissionThenClose(MissionType missionType)
    {
        var services = MainMenuRuntime.Instance?.Services;
        if (services == null) { Close(); return; }

        services.Progress.SelectMissionType(missionType);

        var result = services.MissionStart.TryStartSelected();

        Destroy(gameObject);

        if (result.Started && result.MissionToLoad?.Scene != null &&
            !string.IsNullOrEmpty(result.MissionToLoad.Scene.ScenePath))
        {
            ScenesLoader.Instance?.LoadScene(result.MissionToLoad.Scene.ScenePath);
        }
        // Fallback: popup closed, correct tab selected, player presses Play manually.
    }
}
