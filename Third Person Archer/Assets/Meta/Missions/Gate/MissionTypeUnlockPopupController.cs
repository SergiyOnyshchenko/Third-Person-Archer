using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup for mission type unlocks (Contracts, Sniper).
/// Displays a title, body text, icon image, and a close button.
/// Stays visible until the player clicks the button — no auto-close.
///
/// Setup:
/// 1. Duplicate the Celebration Popup Screen prefab (or create a new one).
/// 2. Add an Image component for the icon.
/// 3. Attach this component, wire all fields.
/// 4. Register in ScreenRegistry Popups under ID "mission_type_unlock_popup".
/// </summary>
public sealed class MissionTypeUnlockPopupController : MonoBehaviour, IReceivesArgs<MissionTypeUnlockPopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bodyText;

    [Header("Icon")]
    [SerializeField] private Image _iconImage;

    [Header("Button")]
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnClose);
    }

    public bool ValidateArgs(MissionTypeUnlockPopupArgs args) => args != null;

    public void ApplyArgs(MissionTypeUnlockPopupArgs args)
    {
        if (_titleText != null) _titleText.text = args.Title;
        if (_bodyText  != null) _bodyText.text  = args.Body;

        if (_iconImage != null)
        {
            _iconImage.sprite  = args.Icon;
            _iconImage.enabled = args.Icon != null;
        }
    }

    private void OnClose()
    {
        Destroy(gameObject);
    }
}
