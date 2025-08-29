using UnityEngine;
using Meta.Economy;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponInfoPanelView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMPro.TMP_Text weaponNameText;
        [SerializeField] private TMPro.TMP_Text weaponTypeText;

        [Header("Stats")]
        [SerializeField] private WeaponStatsPanelView statsPanelView;

        [Header("Context Buttons")]
        [SerializeField] private EquipButtonView equipButtonView;   // visual state handled internally
        [SerializeField] private UnityEngine.UI.Button upgradeButton; // appears only when selected == equipped

        [Header("Purchase")]
        [SerializeField] private PurchaseButtonView purchaseButtonView; // single, generic
        [SerializeField] private CurrencyVisualLibrary currencyVisuals; // provides "Coins" name + icon for CurrencyType you pass

        // Public callbacks (raised to presenter)
        public System.Action OnUpgrade;
        public System.Action<Meta.Economy.CurrencyType, int> OnPurchase; // (currency, price)
        public System.Action OnEquip;

        public void SetHeader(string name, string type)
        {
            if (weaponNameText) weaponNameText.text = name;
            if (weaponTypeText) weaponTypeText.text = type;
        }

        public void SetStats(UIStatNormalizationConfig normalization, UIStyleConfig style, WeaponStats selected, bool showComparison, WeaponStats equipped)
        {
            statsPanelView?.Set(normalization, style, selected, showComparison, equipped);
        }

        /// <summary>
        /// Controls visibility and state of: Equip, Upgrade, Purchase.
        /// - Equip visible only when weapon is owned and not equipped.
        /// - Upgrade visible only when selected == equipped.
        /// - Purchase visible only when not owned (config provided separately).
        /// </summary>
        public void ShowContextOwned(bool owned, bool isEquipped, bool selectedIsEquipped)
        {
            // Equip visuals
            if (equipButtonView)
            {
                bool showEquip = owned; // show even when equipped, but grayed out
                equipButtonView.SetVisible(showEquip);
                equipButtonView.SetStateEquipped(isEquipped);
            }

            // Upgrade button (selected == equipped only)
            if (upgradeButton)
            {
                upgradeButton.gameObject.SetActive(selectedIsEquipped);
                upgradeButton.interactable = selectedIsEquipped;
            }

            // Purchase shown only when not owned; ConfigurePurchase will finalize details
            if (purchaseButtonView)
            {
                purchaseButtonView.gameObject.SetActive(!owned);
            }
        }

        /// <summary>
        /// Configure the single generic purchase button.
        /// Example call: ConfigurePurchase(CurrencyType.Cash, price, visible:!owned, affordable, "Get Now")
        /// </summary>
        public void ConfigurePurchase(Meta.Economy.CurrencyType currency, int price, bool visible, bool affordable, string label = "Get Now")
        {
            if (!purchaseButtonView) return;
            purchaseButtonView.Configure(currency, price, visible, affordable, currencyVisuals, label);
        }

        private void Awake()
        {
            if (upgradeButton) upgradeButton.onClick.AddListener(() => OnUpgrade?.Invoke());
            if (equipButtonView) equipButtonView.OnEquipClicked = () => OnEquip?.Invoke();
            if (purchaseButtonView) purchaseButtonView.OnPurchaseRequested += (c, p) => OnPurchase?.Invoke(c, p);
        }
    }
}
