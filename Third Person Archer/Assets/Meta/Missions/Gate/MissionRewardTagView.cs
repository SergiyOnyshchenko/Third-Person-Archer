using Meta.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal reward tag: currency icon + display name label.
/// No amount shown — used for informational "you earn this type" display.
///
/// Setup: Create a prefab with an Image (icon) and a TextMeshProUGUI (label).
/// Wire both fields. The prefab is instantiated by MissionInfoPopupController
/// into its _rewardTagsRoot.
///
/// Recommended layout: vertical group with icon on top and label below,
/// or horizontal with icon left and label right — designer's choice.
/// </summary>
public sealed class MissionRewardTagView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _label;

    public void Setup(CurrencyType currency, CurrencyVisualLibrary visuals)
    {
        string displayName = currency.ToString();
        Sprite sprite = null;

        if (visuals != null)
            visuals.TryGet(currency, out displayName, out sprite);

        if (_icon != null)
        {
            _icon.sprite = sprite;
            _icon.enabled = sprite != null;
        }

        if (_label != null)
            _label.text = displayName;
    }
}
