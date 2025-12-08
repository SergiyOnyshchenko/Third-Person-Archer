using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public class WeaponPricePanelView : MonoBehaviour
    {
        [Header("Cost Views (instantiate or pre-place them)")]
        [SerializeField] private CostView _purchaseMoneyView;
        [SerializeField] private CostView _purchaseTokenView;
        [SerializeField] private CostView _upgradeMoneyView;
        [SerializeField] private CostView _upgradeTokenView;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _adUpgradeButton;

        [Header("Lock Info")]
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private TextMeshProUGUI _lockedText;

        public event Action OnEquipClicked;
        public event Action OnPurchaseClicked;
        public event Action OnUpgradeClicked;
        public event Action OnAdUpgradeClicked;

        private void Awake()
        {
            if (_equipButton != null)
                _equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());
            if (_purchaseButton != null)
                _purchaseButton.onClick.AddListener(() => OnPurchaseClicked?.Invoke());
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
            if (_adUpgradeButton != null)
                _adUpgradeButton.onClick.AddListener(() => OnAdUpgradeClicked?.Invoke());
        }

        public void ShowLocked(string message)
        {
            SetAllButtonsActive(false);
            SetCostsVisible(false);

            if (_lockedRoot != null)
                _lockedRoot.SetActive(true);
            if (_lockedText != null)
                _lockedText.text = message;
        }

        public void ShowPurchase(Sprite moneyIcon, int moneyRequired,
                                 Sprite tokenIcon, int tokenRequired, int tokenOwned)
        {
            if (_lockedRoot != null)
                _lockedRoot.SetActive(false);

            SetAllButtonsActive(false);
            SetCostsVisible(false);

            if (_purchaseButton != null)
                _purchaseButton.gameObject.SetActive(true);

            if (_equipButton != null)
                _equipButton.gameObject.SetActive(false);

            if (_upgradeButton != null)
                _upgradeButton.gameObject.SetActive(false);

            if (_adUpgradeButton != null)
                _adUpgradeButton.gameObject.SetActive(false);

            if (_purchaseMoneyView != null)
            {
                _purchaseMoneyView.gameObject.SetActive(true);
                _purchaseMoneyView.SetMoney(moneyIcon, moneyRequired);
            }

            if (_purchaseTokenView != null)
            {
                _purchaseTokenView.gameObject.SetActive(true);
                _purchaseTokenView.SetToken(tokenIcon, tokenRequired, tokenOwned);
            }
        }

        public void ShowOwnedNotEquipped()
        {
            if (_lockedRoot != null)
                _lockedRoot.SetActive(false);

            SetAllButtonsActive(false);
            SetCostsVisible(false);

            if (_equipButton != null)
                _equipButton.gameObject.SetActive(true);
        }

        public void ShowEquippedUpgradable(Sprite moneyIcon, int moneyRequired,
                                           Sprite tokenIcon, int tokenRequired, int tokenOwned,
                                           bool canUpgrade, bool canUpgradeWithAd)
        {
            if (_lockedRoot != null)
                _lockedRoot.SetActive(false);

            SetAllButtonsActive(false);
            SetCostsVisible(false);

            if (_equipButton != null)
                _equipButton.gameObject.SetActive(false);

            if (_upgradeButton != null)
            {
                _upgradeButton.gameObject.SetActive(true);
                _upgradeButton.interactable = canUpgrade;
            }

            if (_adUpgradeButton != null)
            {
                _adUpgradeButton.gameObject.SetActive(canUpgradeWithAd);
                _adUpgradeButton.interactable = canUpgrade;
            }

            if (_upgradeMoneyView != null)
            {
                _upgradeMoneyView.gameObject.SetActive(true);
                _upgradeMoneyView.SetMoney(moneyIcon, moneyRequired);
            }

            if (_upgradeTokenView != null)
            {
                _upgradeTokenView.gameObject.SetActive(true);
                _upgradeTokenView.SetToken(tokenIcon, tokenRequired, tokenOwned);
            }
        }

        private void SetAllButtonsActive(bool active)
        {
            if (_equipButton != null) _equipButton.gameObject.SetActive(active);
            if (_purchaseButton != null) _purchaseButton.gameObject.SetActive(active);
            if (_upgradeButton != null) _upgradeButton.gameObject.SetActive(active);
            if (_adUpgradeButton != null) _adUpgradeButton.gameObject.SetActive(active);
        }

        private void SetCostsVisible(bool visible)
        {
            if (_purchaseMoneyView != null) _purchaseMoneyView.gameObject.SetActive(visible);
            if (_purchaseTokenView != null) _purchaseTokenView.gameObject.SetActive(visible);
            if (_upgradeMoneyView != null) _upgradeMoneyView.gameObject.SetActive(visible);
            if (_upgradeTokenView != null) _upgradeTokenView.gameObject.SetActive(visible);
        }
    }
}