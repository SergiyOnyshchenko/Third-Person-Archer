using System;
using UnityEngine;

namespace Meta.Economy
{
    /// <summary>
    /// Glue between your generic PurchaseButtonView and the Wallet.
    /// Keeps UI logic out of domain (Single Responsibility).
    /// </summary>
    public sealed class PurchaseButtonPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PurchaseButtonView view; // your existing view
        [SerializeField] private CurrencyVisualLibrary visuals;

        [Header("Offer")]
        [SerializeField] private CurrencyType currency;
        [Min(0)][SerializeField] private int price = 100;
        [SerializeField] private bool hideIfNotAffordable = false;
        [SerializeField] private string overrideLabel = null;

        public event Action OnPurchased; // notify item/upgrade owners

        private void Awake()
        {
            if (!view) view = GetComponent<PurchaseButtonView>();
        }

        private void OnEnable()
        {
            Configure();
            if (Economy.Wallet != null)
                Economy.Wallet.BalanceChanged += HandleBalanceChanged;

            if (view != null)
                view.OnPurchaseRequested += HandlePurchaseRequested;
        }

        private void OnDisable()
        {
            if (Economy.Wallet != null)
                Economy.Wallet.BalanceChanged -= HandleBalanceChanged;

            if (view != null)
                view.OnPurchaseRequested -= HandlePurchaseRequested;
        }

        private void Configure()
        {
            bool canAfford = Economy.Wallet.CanAfford(currency, price);
            bool visible = hideIfNotAffordable ? canAfford : true;

            view.Configure(currency, price, visible, canAfford, visuals, overrideLabel);
        }

        private void HandleBalanceChanged(CurrencyType t, int _, int __)
        {
            if (t == currency) Configure();
        }

        private void HandlePurchaseRequested(CurrencyType t, int p)
        {
            if (t != currency || p != price) return; // safety: view always calls with current values
            if (Economy.Wallet.TrySpend(currency, price))
                OnPurchased?.Invoke();
        }
    }
}
