using System.Collections.Generic;
using System.Linq;
using Meta.Economy;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Weapons.UI
{
    public class WeaponSelectionScreen : MonoBehaviour, IReceivesArgs<WeaponSelectionArgs>
    {
        [Header("Refs")]
        [SerializeField] private WeaponCatalog _weaponCatalog;
        [SerializeField] private MissionProgressData _missionProgressData;
        [SerializeField] private CurrencyVisualLibrary _currencyVisualLibrary;
        [SerializeField] private WeaponClassIconLibrary _weaponClassIconLibrary;
        [SerializeField] private WeaponStatsRangeConfig _statsRangeConfig;

        [Header("Selected Weapon Info")]
        [SerializeField] private TextMeshProUGUI _selectedWeaponNameText;
        [SerializeField] private TextMeshProUGUI _selectedWeaponClassText;

        [Header("UI")]
        [SerializeField] private WeaponStatsPanelView _statsPanel;
        [SerializeField] private WeaponPricePanelView _pricePanel;
        [SerializeField] private WeaponUpgradeLevelView _upgradeLevelView;
        [SerializeField] private WeaponListItemView _itemPrefab;
        [SerializeField] private Transform _itemsRoot;

        [Header("Debug / Temp")]
        [SerializeField] private int _currentCampaignLevel = 0;

        private WeaponsInitializer _weaponsInitializer;
        private IWeaponRepository _weaponRepo;
        private IStatsService _statsService;
        private IEquipmentService _equipmentService;
        private IUpgradeService _upgradeService;
        private IWallet _wallet;

        private WeaponClass _currentClass;
        private List<WeaponDef> _classWeapons = new();
        private WeaponDef _selectedDef;
        private WeaponInstance _selectedInstance;
        private string _pendingPreselectWeaponId;
        private WeaponHighlightMode _highlightMode;
        private string _highlightTargetWeaponId;

        private readonly List<WeaponListItemView> _spawnedItems = new();
        public UnityEvent<WeaponDef> OnWeaponSelected = new UnityEvent<WeaponDef>();

        private void Awake()
        {
            _weaponsInitializer = WeaponsInitializer.Instance;

            _weaponRepo = _weaponsInitializer.WeaponRepository;
            _statsService = _weaponsInitializer.StatsService;
            _equipmentService = _weaponsInitializer.EquipmentService;
            _upgradeService = _weaponsInitializer.UpgradeService;
            _wallet = Meta.Economy.Economy.Wallet;

            if (_pricePanel != null)
            {
                _pricePanel.OnEquipClicked += HandleEquipClicked;
                _pricePanel.OnPurchaseClicked += HandlePurchaseClicked;
                _pricePanel.OnUpgradeClicked += HandleUpgradeClicked;
                _pricePanel.OnAdUpgradeClicked += HandleAdUpgradeClicked;
            }
        }

        private void OnDestroy()
        {
            if (_pricePanel != null)
            {
                _pricePanel.OnEquipClicked -= HandleEquipClicked;
                _pricePanel.OnPurchaseClicked -= HandlePurchaseClicked;
                _pricePanel.OnUpgradeClicked -= HandleUpgradeClicked;
                _pricePanel.OnAdUpgradeClicked -= HandleAdUpgradeClicked;
            }
        }

        // --- IReceivesArgs<WeaponSelectionArgs> implementation ---

        public bool ValidateArgs(WeaponSelectionArgs args)
        {
            // Simple validation: you can add more if needed.
            return args != null;
        }

        public void ApplyArgs(WeaponSelectionArgs args)
        {
            _pendingPreselectWeaponId = args.PreselectWeaponId;
            _currentCampaignLevel = args.CampaignLevel;
            _highlightMode = args.HighlightMode;
            _highlightTargetWeaponId = args.PreselectWeaponId;
            ShowForClass(args.WeaponClass, args.CampaignLevel);
        }

        /// <summary>
        /// Called from ApplyArgs (via navigator) or manually if needed.
        /// </summary>
        public void ShowForClass(WeaponClass weaponClass, int currentCampaignLevel)
        {
            _currentClass = weaponClass;
            _currentCampaignLevel = currentCampaignLevel;

            gameObject.SetActive(true);
            BuildList();
            SelectInitialWeapon();
        }

        // --- existing code below unchanged ---
        private void BuildList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _spawnedItems.Clear();
            _classWeapons.Clear();

            var allDefs = _weaponCatalog.All.ToArray();
            _classWeapons = allDefs
                .Where(d => d.Class == _currentClass)
                .OrderBy(d => d.UnlockAfterCampaignLevel)
                .ToList();

            var state = _weaponRepo.Load();

            foreach (var def in _classWeapons)
            {
                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id && w.Owned);
                bool isOwned = inst != null && inst.Owned;
                bool isLocked = _currentCampaignLevel < def.UnlockAfterCampaignLevel;
                bool isEquipped = _equipmentService.GetEquippedWeaponId(state, _currentClass) == def.Id;

                var stats = isOwned ? _statsService.Compute(def, inst) : def.BaseStats;

                var classIcon = GetWeaponClassIcon(def.Class);

                var item = Instantiate(_itemPrefab, _itemsRoot);
                item.SetData(def.DisplayName, stats.Damage, isLocked, isEquipped, classIcon,
                    () => OnItemClicked(def));
                _spawnedItems.Add(item);
            }
        }

        private void SelectInitialWeapon()
        {
            if (!string.IsNullOrEmpty(_pendingPreselectWeaponId))
            {
                var target = _classWeapons.FirstOrDefault(d => d != null && d.Id == _pendingPreselectWeaponId);
                _pendingPreselectWeaponId = null;

                if (target != null)
                {
                    SelectWeapon(target);
                    return;
                }
            }

            var state = _weaponRepo.Load();
            var equippedId = _equipmentService.GetEquippedWeaponId(state, _currentClass);

            var initial = _classWeapons.FirstOrDefault(d => d.Id == equippedId)
                          ?? _classWeapons.FirstOrDefault();

            if (initial != null)
                SelectWeapon(initial);
        }

        private void OnItemClicked(WeaponDef def)
        {
            // Manual selection clears guidance highlight.
            if (def != null && def.Id != _highlightTargetWeaponId)
                _highlightMode = WeaponHighlightMode.None;

            SelectWeapon(def);
        }

        private void SelectWeapon(WeaponDef def)
        {
            _selectedDef = def;

            if (_selectedWeaponNameText != null)
                _selectedWeaponNameText.text = def.DisplayName;

            if (_selectedWeaponClassText != null)
                _selectedWeaponClassText.text = def.Class.ToString();

            for (int i = 0; i < _classWeapons.Count; i++)
            {
                bool selected = _classWeapons[i] == def;
                _spawnedItems[i].SetSelected(selected);
            }

            var state = _weaponRepo.Load();
            _selectedInstance = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id && w.Owned);

            bool isLocked = _currentCampaignLevel < def.UnlockAfterCampaignLevel;
            bool isOwned = _selectedInstance != null && _selectedInstance.Owned;
            bool isEquipped = _equipmentService.GetEquippedWeaponId(state, _currentClass) == def.Id;

            var currentStats = isOwned
                ? _statsService.Compute(def, _selectedInstance)
                : def.BaseStats;

            var minStats = _statsRangeConfig != null ? _statsRangeConfig.Min : WeaponStats.Zero;
            var maxStats = _statsRangeConfig != null ? _statsRangeConfig.Max : def.MaxStats;
            _statsPanel.SetCurrentStats(currentStats, minStats, maxStats);

            if (isOwned && isEquipped && def.MaxUpgradeLevel > 0 &&
                _selectedInstance.UpgradeLevel < def.MaxUpgradeLevel)
            {
                var nextLevel = _selectedInstance.UpgradeLevel + 1;
                var nextStats = _statsService.Compute(def, nextLevel);
                _statsPanel.SetUpgradePreview(currentStats, nextStats, true);
            }
            else
            {
                _statsPanel.SetUpgradePreview(currentStats, currentStats, false);
            }

            if (isLocked)
            {
                string formatted = CampaignLevelFormatUtility.FormatCompanyLevelAsZoneCampaign(
                    _missionProgressData, def.UnlockAfterCampaignLevel);

                string msg = $"Unlocks at {formatted}";
                _pricePanel.ShowLocked(msg);
            }
            else if (!isOwned)
            {
                ShowPurchaseUI(def);
            }
            else if (isOwned && !isEquipped)
            {
                _pricePanel.ShowOwnedNotEquipped();
            }
            else
            {
                ShowUpgradeUI(def, _selectedInstance);
            }

            UpdateUpgradeLevelUI(def, _selectedInstance, isOwned);

            ApplyHighlight();

            OnWeaponSelected?.Invoke(def);
        }

        private void ApplyHighlight()
        {
            if (_pricePanel == null) return;

            switch (_highlightMode)
            {
                case WeaponHighlightMode.Upgrade:
                    _pricePanel.PulseUpgradeButton();
                    break;
                case WeaponHighlightMode.Buy:
                    _pricePanel.PulseBuyButton();
                    break;
                default:
                    _pricePanel.StopPulse();
                    break;
            }
        }

        private void ShowPurchaseUI(WeaponDef def)
        {
            int cashRequired = def.PurchaseCash;
            int tokensRequired = def.PurchaseTokens;

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);
            int tokenOwned = _wallet.Get(tokenCurrency);

            var moneyIcon = GetCurrencyIcon(CurrencyType.Cash);
            var tokenIcon = GetCurrencyIcon(tokenCurrency);

            _pricePanel.ShowPurchase(moneyIcon, cashRequired, tokenIcon, tokensRequired, tokenOwned);
        }

        private void ShowUpgradeUI(WeaponDef def, WeaponInstance inst)
        {
            if (def.MaxUpgradeLevel <= 0)
            {
                _pricePanel.ShowOwnedNotEquipped();
                return;
            }

            var profile = def.UpgradePriceProfile;
            if (profile == null)
            {
                _pricePanel.ShowOwnedNotEquipped();
                return;
            }

            bool canUpgrade = _upgradeService.CanUpgrade(def.Id);
            int currentLevel = inst.UpgradeLevel;

            profile.EvaluateUpgradeCost(currentLevel, def.MaxUpgradeLevel,
                                        out var cashRequired, out var tokensRequired);

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);
            int tokenOwned = _wallet.Get(tokenCurrency);

            var moneyIcon = GetCurrencyIcon(CurrencyType.Cash);
            var tokenIcon = GetCurrencyIcon(tokenCurrency);

            bool canUpgradeWithAd = profile.CanUpgradeWithAd;

            _pricePanel.ShowEquippedUpgradable(moneyIcon, cashRequired,
                                               tokenIcon, tokensRequired, tokenOwned,
                                               canUpgrade, canUpgradeWithAd);
        }

        private void UpdateUpgradeLevelUI(WeaponDef def, WeaponInstance inst, bool isOwned)
        {
            if (_upgradeLevelView == null)
                return;

            // Show only when the weapon is owned and has upgrades.
            bool visible = isOwned && def != null && def.MaxUpgradeLevel > 0;

            int currentLevel = 0;
            if (visible && inst != null)
                currentLevel = inst.UpgradeLevel;

            _upgradeLevelView.SetLevel(currentLevel, def != null ? def.MaxUpgradeLevel : 0, visible);
        }

        private Sprite GetCurrencyIcon(CurrencyType currency)
        {
            if (_currencyVisualLibrary != null &&
                _currencyVisualLibrary.TryGet(currency, out _, out var icon))
            {
                return icon;
            }

            return null;
        }

        private Sprite GetWeaponClassIcon(WeaponClass cls)
        {
            if (_weaponClassIconLibrary != null &&
                _weaponClassIconLibrary.TryGet(cls, out var icon))
            {
                return icon;
            }

            return null;
        }

        // Button handlers and SetCampaignLevel() stay as they were...
        private void HandleEquipClicked()
        {
            if (_selectedDef == null) return;

            if (_equipmentService.Equip(_currentClass, _selectedDef.Id))
            {
                BuildList();
                SelectWeapon(_selectedDef);
            }
        }

        private void HandlePurchaseClicked()
        {
            if (_selectedDef == null) return;

            _highlightMode = WeaponHighlightMode.None;

            var def = _selectedDef;
            var state = _weaponRepo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);

            int cashRequired = def.PurchaseCash;
            int tokensRequired = def.PurchaseTokens;

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);

            if (!_wallet.CanAfford(CurrencyType.Cash, cashRequired)) return;
            if (!_wallet.CanAfford(tokenCurrency, tokensRequired)) return;

            _wallet.Spend(CurrencyType.Cash, cashRequired);
            _wallet.Spend(tokenCurrency, tokensRequired);

            if (inst == null)
            {
                inst = new WeaponInstance
                {
                    WeaponId = def.Id,
                    Owned = true,
                    UpgradeLevel = 0
                };
                state.Weapons.Add(inst);
            }
            else
            {
                inst.Owned = true;
            }

            _weaponRepo.Save(state);

            SelectWeapon(def);
        }

        private void HandleUpgradeClicked()
        {
            if (_selectedDef == null) return;

            if (_upgradeService.TryUpgradeWithCurrencies(_selectedDef.Id))
            {
                _highlightMode = WeaponHighlightMode.None;
                SelectWeapon(_selectedDef);
            }
        }

        private void HandleAdUpgradeClicked()
        {
            if (_selectedDef == null) return;

            if (_upgradeService.TryUpgradeWithAd(_selectedDef.Id))
            {
                _highlightMode = WeaponHighlightMode.None;
                SelectWeapon(_selectedDef);
            }
        }

        public void SetCampaignLevel(int campaignLevel)
        {
            _currentCampaignLevel = campaignLevel;
        }
    }
}