using Meta.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CurrencyAmountRowView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private CurrencyVisualLibrary _visuals;

    private CurrencyType _currency;

    public CurrencyType Currency => _currency;

    public void Setup(CurrencyType currency, int amount)
    {
        _currency = currency;

        if (_visuals != null && _visuals.TryGet(currency, out _, out var sprite))
        {
            if (_icon != null) _icon.sprite = sprite;
        }

        SetAmount(amount);
    }

    public void SetAmount(int amount)
    {
        if (_amountText != null)
            _amountText.text = Mathf.Max(0, amount).ToString();
    }
}