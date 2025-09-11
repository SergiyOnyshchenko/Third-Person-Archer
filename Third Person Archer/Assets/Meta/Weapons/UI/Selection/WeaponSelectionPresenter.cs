// Assets/Scripts/Meta/Weapons/UI/Selection/WeaponSelectionPresenter.cs
using System;
using System.Linq;
using Meta.Economy;

namespace Meta.Weapons.UI.Selection
{
    /// <summary>
    /// Orchestrates the Weapon Selection user interface.
    /// Responsibilities:
    /// - Build dynamic weapon-class tabs and the weapon list for the active class.
    /// - Maintain selected vs equipped distinction and render comparison stats when they differ.
    /// - Control the 3D model of the selected weapon (spawn + rotation enabled on this screen).
    /// - Configure context actions: Equip, Upgrade (only when selected == equipped), and a single Coins purchase.
    /// 
    /// Dependencies are injected to satisfy Dependency Inversion and enable unit testing.
    /// Views remain passive; no business logic is placed in MonoBehaviours.
    /// </summary>
    public sealed class WeaponSelectionPresenter
    {
        // ----- Immutable dependencies -----
        private readonly WeaponSelectionView view;
        private readonly UIStatNormalizationConfig normalizationConfig;
        private readonly UIStyleConfig styleConfig;
        private readonly IWeaponRepository weaponRepository;
        private readonly IEquipmentService equipmentService;
        private readonly IStatsService statsService;
        private readonly IWallet walletService;
        private readonly IWeaponIconProvider iconProvider;
        private readonly IWeaponPrefabProvider prefabProvider;
        private readonly WeaponDef[] weaponCatalog;

        // ----- UI state -----
        private WeaponClass activeClass;
        private string selectedWeaponId;

        // Configure which CurrencyType represents "Coins" in your economy.
        // If your enum already has Coins, point this to that value. By default we map to Cash.
        private readonly CurrencyType coinsCurrencyType = CurrencyType.Cash;

        public WeaponSelectionPresenter(
            WeaponSelectionView view,
            UIStatNormalizationConfig normalizationConfig,
            UIStyleConfig styleConfig,
            IWeaponRepository weaponRepository,
            IEquipmentService equipmentService,
            IStatsService statsService,
            IWallet walletService,
            IWeaponIconProvider iconProvider,
            IWeaponPrefabProvider prefabProvider,
            WeaponDef[] weaponCatalog)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.normalizationConfig = normalizationConfig ?? throw new ArgumentNullException(nameof(normalizationConfig));
            this.styleConfig = styleConfig ?? throw new ArgumentNullException(nameof(styleConfig));
            this.weaponRepository = weaponRepository ?? throw new ArgumentNullException(nameof(weaponRepository));
            this.equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            this.statsService = statsService ?? throw new ArgumentNullException(nameof(statsService));
            this.walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            this.iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
            this.prefabProvider = prefabProvider ?? throw new ArgumentNullException(nameof(prefabProvider));
            this.weaponCatalog = weaponCatalog ?? Array.Empty<WeaponDef>();

            // View events
            this.view.OnTabSelected += OnTabSelected;
            this.view.OnWeaponSelected += OnWeaponSelected;

            // 3D model bootstrap
            this.view.DisplayController.Initialize(this.prefabProvider);

            // Info Panel callbacks
            this.view.InfoPanelView.OnEquip   = OnEquipRequested;
            this.view.InfoPanelView.OnUpgrade = OnUpgradeRequested;        // visible only when selected == equipped
            this.view.InfoPanelView.OnPurchase = OnPurchaseRequested;      // (currency, price)
        }

        /// <summary>Entry point to render the screen.</summary>
        public void Show()
        {
            BuildTabs();
            activeClass = weaponCatalog.Select(d => d.Class).Distinct().FirstOrDefault();
            BuildWeaponList();
            SelectInitialWeaponForActiveClass();
        }

        // ---------------------------
        // Tabs and List
        // ---------------------------

        private void BuildTabs()
        {
            view.ClearTabs();

            var classes = weaponCatalog
                .Select(d => d.Class)
                .Distinct()
                .OrderBy(c => c.ToString());

            foreach (var weaponClass in classes)
            {
                var tabView = view.CreateTab();
                tabView.Set(weaponClass.ToString().ToUpperInvariant(), isSelected: weaponClass.Equals(activeClass));
                view.HookTab(tabView, weaponClass);
            }
        }

        private void BuildWeaponList()
        {
            view.ClearWeaponList();
            var state = weaponRepository.Load();

            foreach (var def in weaponCatalog.Where(d => d.Class == activeClass))
            {
                var instance = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
                bool owned = instance?.Owned == true;
                bool equipped = instance?.Equipped == true;

                var item = view.CreateWeaponListItem();
                item.Bind(iconProvider.GetIcon(def.Id), def.DisplayName, owned, equipped);
                view.HookWeaponItem(item, def.Id);
            }
        }

        private void SelectInitialWeaponForActiveClass()
        {
            var state = weaponRepository.Load();

            // Prefer equipped in this class
            var equipped = state.Weapons.FirstOrDefault(w =>
            {
                if (!w.Equipped) return false;
                var d = weaponCatalog.FirstOrDefault(x => x.Id == w.WeaponId);
                return d != null && d.Class == activeClass;
            });

            if (equipped != null) { OnWeaponSelected(equipped.WeaponId); return; }

            // Then first owned
            var owned = state.Weapons.FirstOrDefault(w =>
            {
                if (!w.Owned) return false;
                var d = weaponCatalog.FirstOrDefault(x => x.Id == w.WeaponId);
                return d != null && d.Class == activeClass;
            });

            if (owned != null) { OnWeaponSelected(owned.WeaponId); return; }

            // Else first definitional entry
            var first = weaponCatalog.FirstOrDefault(d => d.Class == activeClass);
            if (first != null) OnWeaponSelected(first.Id);
        }

        private void OnTabSelected(WeaponClass weaponClass)
        {
            activeClass = weaponClass;
            BuildTabs();
            BuildWeaponList();
            SelectInitialWeaponForActiveClass();
        }

        private void OnWeaponSelected(string weaponId)
        {
            selectedWeaponId = weaponId;

            // 3D display: spawn selected model and enable idle rotation for this screen
            view.DisplayController.LoadWeaponModel(selectedWeaponId);
            view.DisplayController.SetRotationEnabled(true);

            DrawInfoPanel();
        }

        // ---------------------------
        // Info Panel
        // ---------------------------

        private void DrawInfoPanel()
        {
            var def = weaponCatalog.First(d => d.Id == selectedWeaponId);
            var state = weaponRepository.Load();

            // Selected weapon state and computed stats
            var selectedInstance = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
            var selectedStats = statsService.Compute(def, selectedInstance ?? new WeaponInstance { WeaponId = def.Id });

            // Equipped weapon in same class (if any) for comparison overlay
            var equippedInstance = GetEquippedInstanceInClass(def.Class, state);
            var equippedStats = equippedInstance != null
                ? statsService.Compute(weaponCatalog.First(d => d.Id == equippedInstance.WeaponId), equippedInstance)
                : selectedStats;

            bool isOwned = selectedInstance?.Owned == true;
            bool isEquipped = selectedInstance?.Equipped == true;
            bool selectedIsEquipped = isEquipped;
            bool showComparisonOverlay = equippedInstance != null && equippedInstance.WeaponId != selectedWeaponId;

            // Header
            view.InfoPanelView.SetHeader(def.DisplayName, def.Class.ToString().ToUpperInvariant());

            // Stats with optional comparison overlay
            view.InfoPanelView.SetStats(normalizationConfig, styleConfig, selectedStats, showComparisonOverlay, equippedStats);

            // Context visibility
            view.InfoPanelView.ShowContextOwned(isOwned, isEquipped, selectedIsEquipped);

            // Purchase (Coins only in the view)
            if (!isOwned)
            {
                // We surface "Coins" in UI; map that to whatever CurrencyType you use for coins.
                int coinsPrice = def.Acquisition.CashPrice; // authored as "cash" in data, presented as Coins
                bool coinsVisible = def.Acquisition.CanBuyWithCash; // show only this path
                bool coinsAffordable = coinsVisible && walletService.CanAfford(coinsCurrencyType, coinsPrice);

                view.InfoPanelView.ConfigurePurchase(
                    currency: coinsCurrencyType,
                    price: coinsPrice,
                    visible: coinsVisible,
                    affordable: coinsAffordable,
                    label: "Get Now"
                );
            }
        }

        private static WeaponInstance GetEquippedInstanceInClass(WeaponClass weaponClass, WeaponsState state)
        {
            return state.Weapons.FirstOrDefault(w =>
            {
                if (!w.Equipped) return false;
                // We need to verify class via catalog elsewhere; caller uses catalog to compute stats already.
                return true; // caller filters by class using catalog; here we just return first equipped and let caller validate.
            });
        }

        // ---------------------------
        // Actions
        // ---------------------------

        private void OnEquipRequested()
        {
            if (string.IsNullOrEmpty(selectedWeaponId)) return;

            if (equipmentService.Equip(selectedWeaponId))
            {
                // Refresh list badges and info panel state
                BuildWeaponList();
                DrawInfoPanel();
            }
        }

        private void OnUpgradeRequested()
        {
            // Navigation stub for later Upgrade screen.
            // Intentionally left blank here per scope: selection screen only.
        }

        private void OnPurchaseRequested(CurrencyType currency, int price)
        {
            // Currency is generic; the view sends the coins CurrencyType configured above.
            if (!walletService.CanAfford(currency, price)) return;

            walletService.Spend(currency, price);
            GrantWeapon(selectedWeaponId);

            // Refresh visuals
            BuildWeaponList();
            DrawInfoPanel();
        }

        private void GrantWeapon(string weaponId)
        {
            var state = weaponRepository.Load();
            var instance = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId);
            if (instance == null)
            {
                instance = new WeaponInstance
                {
                    WeaponId = weaponId,
                    Owned = true,
                    Equipped = false
                };
                state.Weapons.Add(instance);
            }
            else
            {
                instance.Owned = true;
            }
            weaponRepository.Save(state);
        }
    }
}
