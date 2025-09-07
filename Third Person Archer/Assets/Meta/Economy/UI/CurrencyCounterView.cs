using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Economy
{
    /// <summary>
    /// Minimal HUD element that shows a single currency (e.g., Coins, Gold).
    /// </summary>
    public sealed class CurrencyCounterView : MonoBehaviour
    {
        [SerializeField] private CurrencyType currency;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Image iconImage;
        [SerializeField] private CurrencyVisualLibrary visuals; // uses your existing asset

        private void Start()
        {
            if (visuals != null && visuals.TryGet(currency, out _, out var icon) && iconImage)
                iconImage.sprite = icon;

            Refresh();
            Economy.Wallet.BalanceChanged += OnBalanceChanged;
        }

        private void OnDestroy()
        {
            if (Economy.Wallet != null)
                Economy.Wallet.BalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(CurrencyType type, int newBalance, int _)
        {
            if (type == currency) Refresh(newBalance);
        }

        private void Refresh()
        {
            if (amountText) amountText.text = Economy.Wallet.Get(currency).ToString();
        }

        private void Refresh(int value)
        {
            if (amountText) amountText.text = value.ToString();
        }
    }
}