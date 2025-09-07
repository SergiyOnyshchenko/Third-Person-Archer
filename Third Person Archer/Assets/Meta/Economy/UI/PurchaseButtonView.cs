using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Economy
{
    /// <summary>
    /// Reusable purchase button that can represent any currency.
    /// Hook this up in any screen (weapons, shop, offers).
    /// </summary>
    public class PurchaseButtonView : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Button actionButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;         // e.g., "Get Now", "Buy", "Unlock"
        [SerializeField] private Image currencyIconImage;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text currencyNameText;  // optional "Coins", "Gold", etc.

        [Header("Default Text")]
        [SerializeField] private string defaultLabel = "Buy";

        public Action<CurrencyType, int> OnPurchaseRequested; // (currency, price)

        private CurrencyType currentCurrency;
        private int currentPrice;

        public void Configure(CurrencyType currency, int price,
                              bool visible, bool interactable,
                              CurrencyVisualLibrary visuals,
                              string overrideLabel = null)
        {
            currentCurrency = currency;
            currentPrice = price;

            gameObject.SetActive(visible);
            if (!visible) return;

            if (actionButton) actionButton.interactable = interactable;
            if (labelText) labelText.text = string.IsNullOrEmpty(overrideLabel) ? defaultLabel : overrideLabel;
            if (priceText) priceText.text = price.ToString();

            if (visuals != null && visuals.TryGet(currency, out var displayName, out var icon))
            {
                if (currencyNameText) currencyNameText.text = displayName;
                if (currencyIconImage) currencyIconImage.sprite = icon;
            }
            else
            {
                if (currencyNameText) currencyNameText.text = currency.ToString();
                if (currencyIconImage) currencyIconImage.sprite = null;
            }
        }

        private void Awake()
        {
            if (actionButton) actionButton.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            OnPurchaseRequested?.Invoke(currentCurrency, currentPrice);
        }
    }
}
