using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinsForAd : MonoBehaviour
{
    [SerializeField] private int _coinsAmount;
    [SerializeField] private TextMeshProUGUI _textField;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _textField.text = _coinsAmount.ToString();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(Play);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(Play);
    }

    private void Play()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool value) =>
        {
            if (value)
            {
                Economy.Instance.AddToCurrency(CurrencyType.Coin, _coinsAmount);
            }
        });
    }
}
