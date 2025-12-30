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

    [Header("Defaults")]
    [SerializeField] private string _defaultTitle = "Your damage is too low for this mission.";
    [SerializeField] private string _hintFormat = "Upgrade your {0}, or buy a new one.";

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
            _titleText.text = _defaultTitle;

        if (_currentDamageText != null)
            _currentDamageText.text = $"Current: {args.CurrentDamage:0.##}";

        if (_requiredDamageText != null)
            _requiredDamageText.text = $"Required: {args.RequiredDamage:0.##}";

        if (_hintText != null)
            _hintText.text = string.Format(_hintFormat, args.WeaponClass.ToString().ToLowerInvariant());
    }

    private void Close()
    {
        Destroy(gameObject); // Popups are expected to self-destroy :contentReference[oaicite:3]{index=3}
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
        nav.Open(_args.WeaponScreenId, weaponArgs, reuseCached: true); // Open full screen 
    }
}