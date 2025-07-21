using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnlockPopupView : PopupView
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _duration = 2f;
    [SerializeField] private Button _backgroundButton;

    private void Awake()
    {
        if (_backgroundButton != null)
        {
            _backgroundButton.onClick.AddListener(Close);
        }
    }

    public override void Initialize(string message)
    {
        _text.text = message;
    }

    public override float GetDuration() => _duration;
}