using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic celebration/milestone popup. Receives CelebrationPopupArgs via IReceivesArgs.
/// Used for: zone campaign complete, boss victory, contracts milestones.
///
/// Setup: Create a prefab with title text, body text, and a close button.
/// Attach this component, wire the fields, and register the prefab in ScreenRegistry Popups
/// under the ID "CelebrationPopup".
/// </summary>
public sealed class CelebrationPopupController : MonoBehaviour, IReceivesArgs<CelebrationPopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bodyText;

    [Header("Button")]
    [SerializeField] private Button _closeButton;

    [Header("Fallback content (shown if popup is opened without args)")]
    [SerializeField] private string _fallbackTitle = "Milestone!";
    [SerializeField, TextArea(3, 8)] private string _fallbackBody = "";

    private void Awake()
    {
        // Apply fallback text; will be overwritten by ApplyArgs when args are provided
        if (_titleText != null) _titleText.text = _fallbackTitle;
        if (_bodyText != null)  _bodyText.text  = _fallbackBody;

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnClose);
    }

    public bool ValidateArgs(CelebrationPopupArgs args)
    {
        return args != null;
    }

    public void ApplyArgs(CelebrationPopupArgs args)
    {
        if (_titleText != null) _titleText.text = args.Title;
        if (_bodyText != null)  _bodyText.text  = args.Body;
    }

    private void OnClose()
    {
        Destroy(gameObject);
    }
}
