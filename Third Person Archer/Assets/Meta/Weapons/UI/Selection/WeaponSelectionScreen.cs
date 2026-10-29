using System.Collections.Generic;
using System.Linq;
using Meta.Economy;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Arsenal of one class. Tiers are bought in order (previous tier maxed first), the class level
    /// is shared by all models, every owned model (tier or skin) shoots with the best tier's power.
    /// </summary>
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

        [Header("Zone matchup (optional)")]
        [Tooltip("\"Japan · Level 6: Brawler 2 shots → 32 dmg for 1 shot\" for the class's next Campaign level.")]
        [SerializeField] private GameObject _matchupRoot;
        [SerializeField] private TextMeshProUGUI _matchupText;

        [Header("Debug / Temp")]
        [SerializeField] private int _currentCampaignLevel = 0;

        private WeaponsInitializer _weaponsInitializer;
        private IWeaponRepository _weaponRepo;
        private IEquipmentService _equipmentService;
        private IUpgradeService _upgradeService;
        private IClassProgressionService _progression;
        private IWallet _wallet;

        private WeaponClass _currentClass;
        private List<WeaponDef> _classWeapons = new();
        private WeaponDef _selectedDef;
        private string _pendingPreselectWeaponId;
        private WeaponHighlightMode _highlightMode;
        private string _highlightTargetWeaponId;
        private bool _adInProgress;

        private readonly List<WeaponListItemView> _spawnedItems = new();
        public UnityEvent<WeaponDef> OnWeaponSelected = new UnityEvent<WeaponDef>();

        /// <summary>Raised when a class is shown (args, class tabs) or its progress changed (upgrade, purchase).</summary>
        public event System.Action<WeaponClass> OnClassShown;

        public WeaponClass CurrentClass => _currentClass;
        public int CampaignLevel => _currentCampaignLevel;

        private void Awake()
        {
            _weaponsInitializer = WeaponsInitializer.Instance;

            _weaponRepo = _weaponsInitializer.WeaponRepository;
            _equipmentService = _weaponsInitializer.EquipmentService;
            _upgradeService = _weaponsInitializer.UpgradeService;
            _progression = _weaponsInitializer.Progression;
            _wallet = Meta.Economy.Economy.Wallet;

            if (_pricePanel != null)
            {
                _pricePanel.OnEquipClicked += HandleEquipClicked;
                _pricePanel.OnPurchaseClicked += HandlePurchaseClicked;
                _pricePanel.OnUpgradeClicked += HandleUpgradeClicked;
                _pricePanel.OnAdUpgradeClicked += HandleAdClicked;
            }
        }

        private void OnDestroy()
        {
            if (_pricePanel != null)
            {
                _pricePanel.OnEquipClicked -= HandleEquipClicked;
                _pricePanel.OnPurchaseClicked -= HandlePurchaseClicked;
                _pricePanel.OnUpgradeClicked -= HandleUpgradeClicked;
                _pricePanel.OnAdUpgradeClicked -= HandleAdClicked;
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
            OnClassShown?.Invoke(_currentClass);
        }

        private void BuildList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _spawnedItems.Clear();

            // Tiers in order, then skins.
            _classWeapons = _weaponCatalog.All
                .Where(d => d != null && d.Class == _currentClass)
                .OrderBy(d => d.IsSkin ? 1 : 0)
                .ThenBy(d => d.Tier)
                .ToList();

            var state = _weaponRepo.Load();
            var classIcon = GetWeaponClassIcon(_currentClass);

            foreach (var def in _classWeapons)
            {
                bool isEquipped = _equipmentService.GetEquippedWeaponId(state, _currentClass) == def.Id;

                var item = Instantiate(_itemPrefab, _itemsRoot);
                item.SetData(def.DisplayName, GetDisplayStats(def).Damage, IsCampaignLocked(def), isEquipped, classIcon,
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

            var equippedId = _equipmentService.GetEquippedWeaponId(_currentClass);

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
                _selectedWeaponClassText.text = def.IsSkin ? $"{def.Class} skin" : $"{def.Class} tier {def.Tier}";

            for (int i = 0; i < _classWeapons.Count; i++)
                _spawnedItems[i].SetSelected(_classWeapons[i] == def);

            bool isOwned = _progression.IsOwned(def.Id);
            bool isEquipped = _equipmentService.GetEquippedWeaponId(_currentClass) == def.Id;

            var currentStats = GetDisplayStats(def);
            var minStats = _statsRangeConfig != null ? _statsRangeConfig.Min : WeaponStats.Zero;
            var maxStats = _statsRangeConfig != null ? _statsRangeConfig.Max : def.MaxStats;
            _statsPanel.SetCurrentStats(currentStats, minStats, maxStats);

            var power = _progression.GetPowerDef(_currentClass);
            int classLevel = _progression.GetClassLevel(_currentClass);

            if (isOwned && isEquipped && power != null && _upgradeService.CanUpgrade(def.Id))
            {
                var nextStats = _progression.GetTierStats(power, classLevel + 1);
                _statsPanel.SetUpgradePreview(currentStats, nextStats, true);
            }
            else
            {
                _statsPanel.SetUpgradePreview(currentStats, currentStats, false);
            }

            if (!isOwned)
            {
                if (def.IsSkin)
                    ShowSkinUI(def);
                else
                    ShowTierPurchaseUI(def);
            }
            else if (!isEquipped)
            {
                _pricePanel.ShowOwnedNotEquipped();
            }
            else
            {
                ShowUpgradeUI(def);
            }

            UpdateUpgradeLevelUI(power, classLevel);
            RenderMatchup(currentStats.Damage, power, classLevel);

            ApplyHighlight();

            OnWeaponSelected?.Invoke(def);
        }

        // Owned models and skins shoot with the class power; an unowned tier shows its base (the jump on purchase).
        private WeaponStats GetDisplayStats(WeaponDef def)
        {
            if (def.IsSkin || _progression.IsOwned(def.Id))
                return _progression.GetPowerStats(_currentClass);

            return _progression.GetTierStats(def, _progression.GetTierStartLevel(def));
        }

        private bool IsCampaignLocked(WeaponDef def) =>
            !def.IsSkin && _currentCampaignLevel < def.UnlockAfterCampaignLevel;

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

        private void ShowTierPurchaseUI(WeaponDef def)
        {
            var block = _progression.GetTierPurchaseBlock(def, _currentCampaignLevel);
            var previous = _progression.GetTierDef(def.Class, def.Tier - 1);

            switch (block)
            {
                case TierPurchaseBlock.CampaignLocked:
                    string formatted = CampaignLevelFormatUtility.FormatCompanyLevelAsZoneCampaign(
                        _missionProgressData, def.UnlockAfterCampaignLevel);
                    _pricePanel.ShowLocked($"Unlocks at {formatted}");
                    return;

                case TierPurchaseBlock.PreviousTierMissing:
                    _pricePanel.ShowLocked($"Buy {previous?.DisplayName} first");
                    return;

                case TierPurchaseBlock.PreviousTierNotMaxed:
                    int cap = _progression.GetTierStartLevel(previous) + previous.MaxUpgradeLevel;
                    _pricePanel.ShowLocked($"Upgrade {previous.DisplayName} to Lvl{cap + 1}");
                    return;
            }

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);
            _pricePanel.ShowPurchase(GetCurrencyIcon(CurrencyType.Cash), def.PurchaseCash,
                                     GetCurrencyIcon(tokenCurrency), def.PurchaseTokens, _wallet.Get(tokenCurrency));
        }

        private void ShowSkinUI(WeaponDef def)
        {
            int watched = _progression.GetSkinAdsWatched(def);
            bool adReady = !_adInProgress && Meta.Ads.Ads.Service.IsRewardedReady(Meta.Ads.AdPlacements.SkinUnlock);
            _pricePanel.ShowSkinAdUnlock($"Skin: watch ads to unlock {watched}/{def.SkinAdsRequired}", adReady);
        }

        private void ShowUpgradeUI(WeaponDef def)
        {
            if (!_upgradeService.TryGetUpgradeCost(def.Class, out var cashRequired, out var tokensRequired))
            {
                // Class level is at the cap of the best owned tier.
                var next = _progression.GetNextTierDef(def.Class);
                _pricePanel.ShowLocked(next != null ? $"Max level. Next: {next.DisplayName}" : "Max level");
                return;
            }

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);

            _pricePanel.ShowEquippedUpgradable(GetCurrencyIcon(CurrencyType.Cash), cashRequired,
                                               GetCurrencyIcon(tokenCurrency), tokensRequired, _wallet.Get(tokenCurrency),
                                               _upgradeService.CanAffordUpgrade(def.Class),
                                               _upgradeService.CanUpgradeWithAd(def.Id));
        }

        /// <summary>
        /// Next uncompleted Campaign level of this class in the current zone: recommended power vs
        /// the class's power now, and whether the next upgrade gets there.
        /// </summary>
        private void RenderMatchup(float damage, WeaponDef power, int classLevel)
        {
            if (_matchupRoot == null || _matchupText == null) return;

            var services = MainMenuRuntime.Instance != null ? MainMenuRuntime.Instance.Services : null;
            var zone = services?.Progress.CurrentZone;
            var campaign = zone?.GetSegmentByType(MissionType.Campaign);

            int targetIndex = -1;
            global::MissionGateResult gate = null;
            if (campaign != null)
            {
                for (int i = 0; i < campaign.Missions.Count; i++)
                {
                    var m = campaign.Missions[i];
                    if (services.ProgressData.IsCampaignLevelCleared(zone, m)) continue;

                    var ctx = services.BuildContextFor(MissionType.Campaign, m);
                    if (ctx.RequiredWeaponClass != _currentClass) continue;

                    targetIndex = i;
                    gate = services.Gate.CheckCampaignGate(ctx);
                    break;
                }
            }

            _matchupRoot.SetActive(gate != null);
            if (gate == null) return;

            int r = Mathf.Max(1, Mathf.FloorToInt(gate.RecommendedDamage));
            int dmg = Mathf.FloorToInt(damage);

            string level = services.ProgressData.IsHeroicUnlocked() ? "Heroic" : "Level";
            string head = $"{zone.ZoneName} · {level} {targetIndex + 1}: ";
            string green = MissionPreviewUtility.Hex(MissionPreviewUtility.Green);
            string yellow = MissionPreviewUtility.Hex(MissionPreviewUtility.Yellow);

            if (dmg >= r)
            {
                _matchupText.text = head + $"<color={green}>ready!</color>";
                return;
            }

            string text = head + $"power <color={yellow}>{dmg} / {r}</color>";

            if (power != null && _upgradeService.CanUpgrade(power.Id))
            {
                int next = Mathf.FloorToInt(_progression.GetTierStats(power, classLevel + 1).Damage);
                if (next >= r) text += $"\n<color={green}>Next upgrade gets you there!</color>";
            }

            _matchupText.text = text;
        }

        private void UpdateUpgradeLevelUI(WeaponDef power, int classLevel)
        {
            if (_upgradeLevelView == null)
                return;

            bool visible = power != null && power.MaxUpgradeLevel > 0;
            int start = visible ? _progression.GetTierStartLevel(power) : 0;
            int cap = visible ? start + power.MaxUpgradeLevel : 0;

            _upgradeLevelView.SetLevel(classLevel, start, cap, visible);
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

        private void Reselect(WeaponDef def)
        {
            BuildList();
            SelectWeapon(def);
            OnClassShown?.Invoke(_currentClass);
        }

        private void HandleEquipClicked()
        {
            if (_selectedDef == null) return;

            if (_equipmentService.Equip(_currentClass, _selectedDef.Id))
                Reselect(_selectedDef);
        }

        private void HandlePurchaseClicked()
        {
            if (_selectedDef == null) return;

            _highlightMode = WeaponHighlightMode.None;

            if (_progression.TryBuyTier(_selectedDef, _currentCampaignLevel))
                Reselect(_selectedDef);
        }

        private void HandleUpgradeClicked()
        {
            if (_selectedDef == null) return;

            if (_upgradeService.TryUpgradeWithCurrencies(_selectedDef.Id))
            {
                _highlightMode = WeaponHighlightMode.None;
                Reselect(_selectedDef);
            }
        }

        // The ad button unlocks skins (unowned skin selected) or tops up an upgrade.
        private void HandleAdClicked()
        {
            if (_selectedDef == null || _adInProgress) return;

            bool skinUnlock = _selectedDef.IsSkin && !_progression.IsOwned(_selectedDef.Id);
            if (!skinUnlock && !_upgradeService.CanUpgradeWithAd(_selectedDef.Id)) return;

            string placement = skinUnlock ? Meta.Ads.AdPlacements.SkinUnlock : Meta.Ads.AdPlacements.WeaponUpgrade;
            var ads = Meta.Ads.Ads.Service;
            if (!ads.IsRewardedReady(placement)) return;

            var def = _selectedDef;
            _adInProgress = true;
            ads.ShowRewarded(placement, success =>
            {
                _adInProgress = false;
                if (this == null) return;

                if (success)
                {
                    if (skinUnlock)
                        _progression.RegisterSkinAd(def);
                    else if (_upgradeService.TryUpgradeWithAd(def.Id))
                        _highlightMode = WeaponHighlightMode.None;
                }

                if (_selectedDef == def)
                    Reselect(def);
            });
        }

        public void SetCampaignLevel(int campaignLevel)
        {
            _currentCampaignLevel = campaignLevel;
        }
    }
}
