using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shown once when the whole campaign is complete (last level of Zone 3, which has no Boss):
/// Heroic mode is unlocked.
/// Needs a prefab wired with title, body, and close button.
/// Register the prefab in ScreenRegistry Popups as "LoopTransitionPopup".
/// </summary>
public sealed class LoopTransitionPopupController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bodyText;

    [Header("Button")]
    [SerializeField] private Button _closeButton;

    [Header("Content (edit in Inspector)")]
    [SerializeField] private string _titleString = HeroicTitle;

    [SerializeField]
    [TextArea(4, 10)]
    private string _bodyString = HeroicBody;

    public const string HeroicTitle = "Campaign Complete!";
    public const string HeroicBody =
        "All three zones cleared!\n\n" +
        "<color=#FF6A50>HEROIC MODE</color> is open: tougher enemies, bigger rewards, Tier 4 weapons.\n\n" +
        "Switch zones with the map arrows.";

    private void Awake()
    {
        if (_titleText != null) _titleText.text = _titleString;
        if (_bodyText != null) _bodyText.text = _bodyString;
        if (_closeButton != null) _closeButton.onClick.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        if (_closeButton != null) _closeButton.onClick.RemoveListener(OnClose);
    }

    private void OnClose()
    {
        Destroy(gameObject);
    }
}
