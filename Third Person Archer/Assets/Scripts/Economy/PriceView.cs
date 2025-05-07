using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceView : MonoBehaviour
{
    [SerializeField] private Price _price;
    [Space]
    [SerializeField] private TMP_Text _currencyText;
    [SerializeField] private Image _icon;

    void OnEnable()
    {
        if (_price != null)
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        Currency currency = Economy.Instance.GetCurrency(_price.Currency);

        if (currency == null)
            return;

        _currencyText.text = _price.Value.ToString();
        _icon.sprite = currency.Icon;
    }
}
