using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CurrencyView : MonoBehaviour
{
    [SerializeField] private Currency _currency;
    [Space]
    [SerializeField] private TMP_Text _currencyText;
    [SerializeField] private float _updateDuration = 1.0f;
    private int _current = 0;

    void OnEnable()
    {
        if (_currency != null)
        {
            _currency.OnCurrencyUpdated += UpdateCurrencyText;
            UpdateCurrencyText();
        }
    }

    void OnDisable()
    {
        if (_currency != null)
        {
            _currency.OnCurrencyUpdated -= UpdateCurrencyText;
        }
    }

    private void UpdateCurrencyText()
    {
        DOVirtual.Int(_current, _currency.Amount, _updateDuration, (int value) =>
        {
            _currencyText.text = value.ToString();
        });  
    }
}