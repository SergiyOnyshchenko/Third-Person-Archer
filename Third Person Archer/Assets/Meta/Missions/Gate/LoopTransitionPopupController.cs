using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple popup shown once when the player completes the last available zone
/// (Zone 3, which has no Boss). Explains that a new loop/challenge begins.
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
    [SerializeField] private string _titleString = "Campaign Complete!";

    [SerializeField]
    [TextArea(4, 10)]
    private string _bodyString =
        "You have cleared all three zones.\n\n" +
        "A new challenge begins — gates are stronger, but rewards are better.\n\n" +
        "Your weapons and progress carry over. Keep fighting!";

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
