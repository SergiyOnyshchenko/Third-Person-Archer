using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [Tooltip("What Play starts, e.g. \"Level 5 · Bow\" (optional).")]
    [SerializeField] private TMP_Text _subtitle;

    [Header("Power indicator (optional)")]
    [Tooltip("Root shown only for gated missions.")]
    [SerializeField] private GameObject _powerRoot;
    [Tooltip("\"current / recommended\" damage.")]
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private Graphic _powerStatusGraphic;
    [SerializeField] private Color _greenColor = new Color(0.36f, 0.78f, 0.33f);
    [SerializeField] private Color _yellowColor = new Color(0.98f, 0.78f, 0.2f);
    [SerializeField] private Color _redColor = new Color(0.87f, 0.27f, 0.22f);

    public void SetInteractable(bool enabled)
    {
        _button.interactable = enabled;
    }

    public void SetSubtitle(string text)
    {
        if (_subtitle == null) return;
        _subtitle.text = text;
        _subtitle.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    public void BindClick(System.Action callback)
    {
        _button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            _button.onClick.AddListener(() => callback());
        }
    }

    public void SetPower(MissionGateResult gate)
    {
        bool visible = gate != null && gate.RecommendedDamage > 0f;

        if (_powerRoot != null)
            _powerRoot.SetActive(visible);

        if (!visible)
            return;

        if (_powerText != null)
            _powerText.text = $"{gate.CurrentDamage:0} / {gate.RecommendedDamage:0}";

        if (_powerStatusGraphic != null)
        {
            _powerStatusGraphic.color = gate.Status switch
            {
                MissionGateStatus.Green => _greenColor,
                MissionGateStatus.Yellow => _yellowColor,
                _ => _redColor,
            };
        }
    }
}
