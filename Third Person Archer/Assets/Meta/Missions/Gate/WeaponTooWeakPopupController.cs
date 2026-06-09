using Meta.Weapons.UI;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup shown when the next Campaign/Boss gate is blocked and the player can take
/// action right now in the weapon screen (upgrade or buy).
///
/// Action button → opens WeaponSelectionScreen with the required class pre-selected
///                 and the target weapon pre-selected.
/// Later button  → closes popup with no navigation.
///
/// Setup: create prefab, assign all [SerializeField] refs, register in ScreenRegistry
/// under the ID configured in PostMissionNextStepPresenter (default: "weapon_too_weak_popup").
/// </summary>
public sealed class WeaponTooWeakPopupController : MonoBehaviour, IReceivesArgs<WeaponTooWeakPopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bodyText;

    [Header("Buttons")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonLabel;
    [SerializeField] private Button _laterButton;

    private WeaponTooWeakPopupArgs _args;

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

    public bool ValidateArgs(WeaponTooWeakPopupArgs args) => args != null;

    public void ApplyArgs(WeaponTooWeakPopupArgs args)
    {
        _args = args;

        string className = args.RequiredWeaponClass.ToString();

        if (_titleText != null)
            _titleText.text = $"Your {className} is too weak";

        if (_bodyText != null)
            _bodyText.text = "Upgrade it or buy a stronger one to continue.";

        if (_actionButtonLabel != null)
        {
            _actionButtonLabel.text = args.IsUpgrade
                ? $"Upgrade {className}"
                : $"Buy Stronger {className}";
        }
    }

    private void OnActionClicked()
    {
        if (_args == null) { Close(); return; }

        // Destroy popup first so it is off screen before the weapon screen opens.
        Destroy(gameObject);

        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

        var highlightMode = _args.IsUpgrade
            ? WeaponHighlightMode.Upgrade
            : WeaponHighlightMode.Buy;

        var weaponArgs = new WeaponSelectionArgs(
            _args.RequiredWeaponClass,
            _args.CampaignLevel,
            _args.TargetWeaponId,
            highlightMode);

        nav.Open(_args.WeaponScreenId, weaponArgs, reuseCached: true);
    }

    private void Close() => Destroy(gameObject);
}
