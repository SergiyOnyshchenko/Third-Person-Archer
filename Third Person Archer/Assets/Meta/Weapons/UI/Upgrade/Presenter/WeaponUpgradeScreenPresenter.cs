using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI;
using Meta.Weapons.UI.Display;
using Meta.Economy;

namespace Meta.Weapons.UI.Upgrade
{
    public sealed class WeaponUpgradeScreenPresenter
    {
        private readonly WeaponUpgradeScreenView view;
        private readonly UIStatNormalizationConfig normalization;
        private readonly UIStyleConfig style;
        private readonly IWeaponRepository repository;
        private readonly IUpgradeService upgradeService;
        private readonly IEquipmentService equipmentService;
        private readonly IStatsService statsService;
        private readonly IWalletService walletService;
        private readonly ITimeProvider timeProvider;
        private readonly WeaponDef[] catalog;
        private readonly WeaponUpgradeFocusMap focusMap;

        private readonly CurrencyType coinsCurrencyType = CurrencyType.Cash; // map to "Coins" in visuals

        private WeaponDef equippedDef;
        private WeaponInstance equippedInst;
        private string selectedPartId;
        private readonly Dictionary<string, UpgradeOptionItemPresenter> itemPresenters = new();

        // Optional: feedback hook after successful upgrade
        private readonly Action playSuccessFeedback;

        public event Action OnRequestReturnToSelection;

        public WeaponUpgradeScreenPresenter(
            WeaponUpgradeScreenView view,
            UIStatNormalizationConfig normalization,
            UIStyleConfig style,
            IWeaponRepository repository,
            IUpgradeService upgradeService,
            IEquipmentService equipmentService,
            IStatsService statsService,
            IWalletService walletService,
            ITimeProvider timeProvider,
            WeaponDef[] catalog,
            WeaponUpgradeFocusMap focusMap,
            Action playSuccessFeedback = null)
        {
            this.view = view;
            this.normalization = normalization;
            this.style = style;
            this.repository = repository;
            this.upgradeService = upgradeService;
            this.equipmentService = equipmentService;
            this.statsService = statsService;
            this.walletService = walletService;
            this.timeProvider = timeProvider;
            this.catalog = catalog;
            this.focusMap = focusMap;
            this.playSuccessFeedback = playSuccessFeedback;

            this.view.Display.SetRotationEnabled(false);
            this.view.OnUpgradeItemSelected += OnUpgradeItemSelected;
            this.view.OnReturnToSelection += () => OnRequestReturnToSelection?.Invoke();

            this.view.Info.OnPurchase += OnPurchaseRequested; // (currency, price)
            this.view.Info.OnUpgrade = null;                 // not used here
            this.view.Info.OnEquip = null;                 // not used here

            this.upgradeService.OnUpgradeStarted += _ => RefreshAll();
            this.upgradeService.OnUpgradeCompleted += _ => RefreshAll();
        }

        public void Show()
        {
            // Use the globally equipped weapon
            var state = repository.Load();
            var eqId = equipmentService.GetEquippedWeaponId(state);
            if (string.IsNullOrEmpty(eqId))
            {
                Debug.LogWarning("No weapon equipped for upgrade screen.");
                return;
            }

            equippedDef = catalog.FirstOrDefault(d => d.Id == eqId);
            equippedInst = state.Weapons.FirstOrDefault(w => w.WeaponId == eqId);
            if (equippedDef == null || equippedInst == null)
            {
                Debug.LogWarning("Equipped weapon not found in catalog or repository.");
                return;
            }

            // Spawn model and stop rotation
            view.Display.LoadWeaponModel(equippedDef.Id);
            view.Display.SetRotationEnabled(false);

            BuildUpgradeList();
            SelectFirstAvailablePart();
            DrawInfoPanelForSelection();
        }

        private void BuildUpgradeList()
        {
            view.ClearUpgradeList();
            itemPresenters.Clear();

            foreach (var part in equippedDef.Parts) // <- no link unwrap
            {
                var item = view.CreateItem();
                var presenter = new UpgradeOptionItemPresenter(item, equippedDef, part, repository, upgradeService, timeProvider);
                itemPresenters[part.Id] = presenter;
                view.HookItem(item, part.Id);

                bool selected = selectedPartId == part.Id;
                presenter.Refresh(selected);
            }
        }

        private void SelectFirstAvailablePart()
        {
            if (!string.IsNullOrEmpty(selectedPartId) && itemPresenters.ContainsKey(selectedPartId))
                return;

            var first = equippedDef.Parts.FirstOrDefault();
            if (first != null) selectedPartId = first.Id;

            HighlightSelection();
            FocusPoseFor(selectedPartId);
        }

        private void OnUpgradeItemSelected(string partId)
        {
            selectedPartId = partId;
            HighlightSelection();
            FocusPoseFor(partId);
            DrawInfoPanelForSelection();
        }

        private void HighlightSelection()
        {
            foreach (var kv in itemPresenters)
            {
                bool selected = kv.Key == selectedPartId;
                kv.Value.Refresh(selected);
            }
        }

        private void FocusPoseFor(string partId)
        {
            if (string.IsNullOrEmpty(partId)) return;
            if (focusMap != null && focusMap.TryGet(partId, out var e))
            {
                view.Display.SmoothFocusPose(e.LocalPosition, e.LocalEulerAngles, 0.25f);
            }
            else
            {
                // Fallback: small nudge so the player notices focus
                view.Display.NudgeForAttention(new Vector3(0f, 10f, 0f));
            }
        }

        private void DrawInfoPanelForSelection()
        {
            var part = ResolvePart(selectedPartId);
            if (part == null) return;

            // Header
            view.Info.SetHeader(equippedDef.DisplayName, equippedDef.Class.ToString().ToUpperInvariant());

            // Current stats (primary) vs after-upgrade preview (green overlay only)
            var currentStats = statsService.Compute(equippedDef, equippedInst);

            var afterInstance = Clone(equippedInst);
            afterInstance.PartLevels.TryGetValue(part.Id, out var currentLevel);
            if (currentLevel < part.MaxLevel)
                afterInstance.PartLevels[part.Id] = currentLevel + 1; // preview next level

            var afterStats = statsService.Compute(equippedDef, afterInstance);
            view.Info.SetStats(normalization, style, currentStats, showComparison: false, equipped: currentStats); // draws base numbers
            view.Info.GetComponentInChildren<WeaponStatsPanelView>()?.SetUpgradePreview(normalization, style, currentStats, afterStats);

            // Buttons visibility:
            // Hide Equip/Upgrade (selection screen features); show purchase button with "Upgrade" label.
            view.Info.ShowContextOwned(owned: true, isEquipped: true, selectedIsEquipped: false); // ensures purchase enabled area is visible
            ConfigurePurchaseForPart(part, currentLevel);
        }

        private void ConfigurePurchaseForPart(WeaponPartDef part, int currentLevel)
        {
            bool atMax = currentLevel >= part.MaxLevel;

            // Determine coins price from the next level (your PartDef.Levels[currentLevel].CashCost)
            int coinsPrice = 0;
            bool visible = !atMax && part.Levels != null && currentLevel >= 0 && currentLevel < part.Levels.Length;
            if (visible)
            {
                coinsPrice = part.Levels[currentLevel].CashCost; // authored as "cash", presented as Coins
            }

            bool affordable = visible && walletService.CanAfford(coinsCurrencyType, coinsPrice);

            view.Info.ConfigurePurchase(coinsCurrencyType, coinsPrice, visible, affordable, label: "Upgrade");
        }

        private void OnPurchaseRequested(CurrencyType currency, int price)
        {
            var part = ResolvePart(selectedPartId);
            if (part == null) return;

            // Guard: check level/max and affordability
            var state = repository.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == equippedDef.Id);
            inst ??= new WeaponInstance { WeaponId = equippedDef.Id };

            inst.PartLevels.TryGetValue(part.Id, out var level);
            if (level >= part.MaxLevel) return;

            if (!walletService.CanAfford(currency, price)) return;

            // Spend and start upgrade (Coins → timer path; your IUpgradeService should handle duration)
            walletService.Spend(currency, price);
            var job = upgradeService.StartUpgrade(equippedDef.Id, part.Id, UpgradePayment.CashWithTimer);
            if (job == null) return;

            playSuccessFeedback?.Invoke(); // optional Feel/VFX
            RefreshAll();
        }

        private WeaponPartDef ResolvePart(string partId)
        {
            return equippedDef.Parts.FirstOrDefault(p => p != null && p.Id == partId);
        }

        private void RefreshAll()
        {
            // Reload instance to reflect timers/levels
            var state = repository.Load();
            equippedInst = state.Weapons.FirstOrDefault(w => w.WeaponId == equippedDef.Id) ?? equippedInst;

            // Refresh items
            foreach (var kv in itemPresenters) kv.Value.Refresh(kv.Key == selectedPartId);

            // Refresh panel preview
            DrawInfoPanelForSelection();
        }

        public void Tick()
        {
            foreach (var kv in itemPresenters) kv.Value.Tick();
        }

        private static WeaponInstance Clone(WeaponInstance src)
        {
            return new WeaponInstance
            {
                WeaponId = src.WeaponId,
                Owned = src.Owned,
                Equipped = src.Equipped,
                PartLevels = new Dictionary<string, int>(src.PartLevels),
                MasteryTiers = new Dictionary<string, int>(src.MasteryTiers)
            };
        }
    }
}
