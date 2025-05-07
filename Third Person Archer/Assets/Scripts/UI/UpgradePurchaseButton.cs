using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UpgradePurchaseButton : MonoBehaviour
{
    [Header("Coins View")]
    [SerializeField] private CurrencyType _type;
    [SerializeField] private int _price;
    [Space]
    [SerializeField] private GameObject _coinsView;
    [SerializeField] private TextMeshProUGUI _priceField;
    [Header("Ads View")]
    [SerializeField] private GameObject _adsView;
    private Button _button;
    public UnityEvent onPurchaseSuccess;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Economy.Instance.GetCurrency(_type).OnCurrencyUpdated += UpdateButtonView;
        UpdateButtonView();
    }

    private void OnDisable()
    {
        Economy.Instance.GetCurrency(_type).OnCurrencyUpdated -= UpdateButtonView;
    }

    public void SetPrice(int price)
    {
        _price = price;
        UpdateButtonView();
    }

    public void UpdateButtonView()
    {
        bool isCoinsView = HasEnoughCost();

        _coinsView.SetActive(isCoinsView);
        _adsView.SetActive(!isCoinsView);

        if (isCoinsView)
        {
            // Coin purchase view
            _priceField.text = _price.ToString();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(PurchaseWithCoins);
        }
        else
        {
            // Ad purchase view
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(PurchaseWithAds);
        }
    }

    private bool HasEnoughCost()
    {
        return Economy.Instance.GetCurrency(_type).Amount >= _price;
    }

    private void PurchaseWithCoins()
    {
        if (HasEnoughCost())
        {
            Economy.Instance.GetCurrency(_type).Subtract(_price);
            GrantItem();
        }
    }

    private void PurchaseWithAds()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool value) =>
        {
            if (value)
            {
                GrantItem();
            }
        });
    }

    private void GrantItem()
    {
        onPurchaseSuccess.Invoke();
    }
}
