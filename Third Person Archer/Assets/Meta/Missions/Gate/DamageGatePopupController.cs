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

    [Header("Tutorial (one-time, set up in prefab)")]
    [Tooltip("Root GameObject containing the tutorial section. Hidden when TutorialText is null.")]
    [SerializeField] private GameObject _tutorialRoot;
    [Tooltip("TextMeshPro for the tutorial body text.")]
    [SerializeField] private TextMeshProUGUI _tutorialBodyText;

    [Header("Defaults")]
    [SerializeField] private string _defaultTitle = "Your damage is too low for this mission.";

    [Tooltip("Hint shown when the player CAN upgrade the current weapon to pass. {0} = weapon class name.")]
    [SerializeField] private string _hintFormat = "Upgrade your {0}, or buy a new one.";

    [Tooltip("Hint shown when the current weapon is at max tier and a purchase is required. {0} = weapon class name.")]
    [SerializeField] private string _forcedPurchaseHintFormat = "Your {0} is fully upgraded. Buy a stronger weapon of the same class.";

    private DamageGatePopupArgs _args;

    private void Awake()
    {
        if (_closeButton != null) _closeButton.onClick.AddListener(Close);
        if (_goButton != null) _goButton.onClick.AddListener(GoToWeapons);
    }

    private void OnDestroy()
    {
        if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
        if (_goButton != null) _goButton.onClick.RemoveListener(GoToWeapons);
    }

    public bool ValidateArgs(DamageGatePopupArgs args)
    {
        return args != null && !string.IsNullOrEmpty(args.WeaponScreenId);
    }

    public void ApplyArgs(DamageGatePopupArgs args)
    {
        _args = args;

        if (_titleText != null)
            _titleText.text = !string.IsNullOrEmpty(args.CustomTitle) ? args.CustomTitle : _defaultTitle;

        if (_currentDamageText != null)
            _currentDamageText.text = $"{args.CurrentDamage:0.##}";

        if (_requiredDamageText != null)
            _requiredDamageText.text = $"{args.RequiredDamage:0.##}";

        if (_hintText != null)
        {
            string hintTemplate;
            if (!string.IsNullOrEmpty(args.CustomHint))
                hintTemplate = args.CustomHint;
            else if (!args.CanUpgradeToPass)
                hintTemplate = _forcedPurchaseHintFormat;
            else
                hintTemplate = _hintFormat;

            _hintText.text = string.Format(hintTemplate, args.WeaponClass.ToString().ToLowerInvariant());
        }

        bool hasTutorial = !string.IsNullOrEmpty(args.TutorialText);
        if (_tutorialRoot != null) _tutorialRoot.SetActive(hasTutorial);
        if (_tutorialBodyText != null && hasTutorial) _tutorialBodyText.text = args.TutorialText;
    }

    private void Close()
    {
        Destroy(gameObject);
    }

    private void GoToWeapons()
    {
        if (_args == null)
        {
            Close();
            return;
        }

        // Close popup first
        Destroy(gameObject);

        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        // Open weapon screen with required class preselected
        var weaponArgs = new WeaponSelectionArgs(_args.WeaponClass, _args.CampaignLevel);
        nav.Open(_args.WeaponScreenId, weaponArgs, reuseCached: true);
    }
}
