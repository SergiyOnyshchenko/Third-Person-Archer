using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PurchaseButton : MonoBehaviour
{
    [SerializeField] private Price _price;
    [Space]
    [SerializeField] private TextMeshProUGUI _priceField;
    [SerializeField] private Image _enoughCoinsBackground;
    [SerializeField] private Image _notEnoughCoinsBackground;
    private Currency _currency;
    private Button _button;

    public UnityEvent onPurchaseSuccess = new UnityEvent();

    private void Awake()
    {
        _currency = Economy.Instance.GetCurrency(CurrencyType.Coin);
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(PurchaseWithCoins);
        _currency.OnCurrencyUpdated += UpdateView;
        UpdateView();
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(PurchaseWithCoins);
        _currency.OnCurrencyUpdated -= UpdateView;
    }

    private void UpdateView()
    {
        _priceField.text = _price.Value.ToString();

        _enoughCoinsBackground.gameObject.SetActive(HasEnoughCost());
        _enoughCoinsBackground.gameObject.SetActive(!HasEnoughCost());
    }

    private bool HasEnoughCost()
    {
        return _currency.Amount >= _price.Value;
    }

    private void PurchaseWithCoins()
    {
        if (HasEnoughCost())
        {
            _currency.Subtract(_price.Value);
            GrantItem();
        }
    }

    private void GrantItem()
    {
        onPurchaseSuccess.Invoke();
    }
}
