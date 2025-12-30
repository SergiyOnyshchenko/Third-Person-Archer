using Meta.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PriceView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amountText;

    [Header("Visuals")]
    [SerializeField] private CurrencyVisualLibrary _visuals;

    [Header("Optional: affordability coloring")]
    [SerializeField] private bool _tintWhenCantAfford = true;
    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _cantAffordColor = Color.red;

    private CurrencyType _currency;
    private int _amount;

    public void Set(CurrencyType currency, int amount)
    {
        _currency = currency;
        _amount = Mathf.Max(0, amount);

        if (_visuals != null && _visuals.TryGet(currency, out _, out var sprite))
        {
            if (_icon != null) _icon.sprite = sprite;
        }

        if (_amountText != null)
            _amountText.text = _amount.ToString();

        RefreshAffordabilityTint();
    }

    public void Clear()
    {
        if (_icon != null) _icon.sprite = null;
        if (_amountText != null) _amountText.text = string.Empty;
    }

    private void RefreshAffordabilityTint()
    {
        if (_amountText == null) return;

        if (!_tintWhenCantAfford || Economy.Wallet == null || _amount <= 0)
        {
            _amountText.color = _defaultColor;
            return;
        }

        bool canAfford = Economy.Wallet.CanAfford(_currency, _amount);
        _amountText.color = canAfford ? _defaultColor : _cantAffordColor;
    }
}