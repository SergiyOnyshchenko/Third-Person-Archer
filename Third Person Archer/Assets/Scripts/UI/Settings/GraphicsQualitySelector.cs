using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Settings row: Auto / Low / Medium / High, switched with the arrows.</summary>
public class GraphicsQualitySelector : MonoBehaviour
{
    [SerializeField] private Button _previous;
    [SerializeField] private Button _next;
    [SerializeField] private TMP_Text _value;

    private const int OptionCount = 4;

    private void OnEnable()
    {
        _previous.onClick.AddListener(Previous);
        _next.onClick.AddListener(Next);
        Refresh();
    }

    private void OnDisable()
    {
        _previous.onClick.RemoveListener(Previous);
        _next.onClick.RemoveListener(Next);
    }

    private void Previous() => Select(((int)GraphicsQuality.Preference + OptionCount - 1) % OptionCount);

    private void Next() => Select(((int)GraphicsQuality.Preference + 1) % OptionCount);

    private void Select(int option)
    {
        GraphicsQuality.SetPreference((GraphicsPreference)option);
        Refresh();
    }

    private void Refresh()
    {
        _value.text = GraphicsQuality.Preference == GraphicsPreference.Auto
            ? $"Auto ({GraphicsQuality.Tier})"
            : GraphicsQuality.Preference.ToString();
    }
}
