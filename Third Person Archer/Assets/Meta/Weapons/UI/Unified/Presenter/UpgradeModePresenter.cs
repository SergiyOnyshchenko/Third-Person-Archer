using System.Linq;
using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI.Upgrade;
using Meta.Economy;
using Meta.Weapons.UI.Display;

namespace Meta.Weapons.UI
{
    internal sealed class UpgradeModePresenter : IWeaponScreenModeController
    {
        private readonly WeaponScreenPresenter host;
        private readonly WeaponScreenView view;
        private readonly UpgradeBottomBarView bar;

        private readonly UIStatNormalizationConfig normalization;
        private readonly UIStyleConfig style;
        private readonly IWeaponRepository repo;
        private readonly IUpgradeService upgrade;
        private readonly IEquipmentService equip;
        private readonly IStatsService stats;
        private readonly IWallet wallet;
        private readonly ITimeProvider time;
        private readonly WeaponDef[] catalog;
        private readonly WeaponUpgradeFocusMap focusMap;
        private readonly WeaponDisplayPoseLibrary poseLib;
        private readonly WeaponPartFocusLibrary focusLib;

        private WeaponDef def;
        private WeaponInstance inst;
        private string selectedPartId;
        private readonly System.Collections.Generic.Dictionary<string, UpgradeOptionItemPresenter> items = new();

        public UpgradeModePresenter(
            WeaponScreenPresenter host,
            WeaponScreenView view,
            UpgradeBottomBarView bar,
            UIStatNormalizationConfig normalization,
            UIStyleConfig style,
            IWeaponRepository repo,
            IUpgradeService upgrade,
            IEquipmentService equip,
            IStatsService stats,
            IWallet wallet,
            ITimeProvider time,
            WeaponDef[] catalog,
            WeaponUpgradeFocusMap focusMap,
            WeaponDisplayPoseLibrary poseLib,
            WeaponPartFocusLibrary focusLib)
        {
            this.host = host;
            this.view = view;
            this.bar = bar;
            this.normalization = normalization;
            this.style = style;
            this.repo = repo;
            this.upgrade = upgrade;
            this.equip = equip;
            this.stats = stats;
            this.wallet = wallet;
            this.time = time;
            this.catalog = catalog;
            this.focusMap = focusMap;
            this.poseLib = poseLib;
            this.focusLib = focusLib;

            bar.OnItemSelected += OnSelectItem;
            view.Info.OnPurchase += OnPurchaseUpgrade;
            view.Info.OnEquip = null;
            view.Info.OnUpgrade = null;

            this.upgrade.OnUpgradeStarted   += _ => RefreshAll();
            this.upgrade.OnUpgradeCompleted += _ => RefreshAll();
        }

        public void Enter()
        {
            // Ensure we have an equipped weapon; if not, try to equip the selected one (if owned)
            var state = repo.Load();
            var eqId = equip.GetEquippedWeaponId(state);
            if (string.IsNullOrEmpty(eqId) && !string.IsNullOrEmpty(host.SelectedWeaponId))
            {
                equip.Equip(host.SelectedWeaponId);
                eqId = host.SelectedWeaponId;
            }

            if (string.IsNullOrEmpty(eqId))
            {
                Debug.LogWarning("Upgrade mode entered without an equipped weapon.");
                host.SwitchMode(WeaponScreenMode.Selection);
                return;
            }

            def = catalog.FirstOrDefault(d => d.Id == eqId);
            inst = state.Weapons.FirstOrDefault(w => w.WeaponId == eqId);
            if (def == null || inst == null)
            {
                Debug.LogWarning("Equipped weapon not found in catalog or repository.");
                host.SwitchMode(WeaponScreenMode.Selection);
                return;
            }

            view.Display.LoadWeaponModel(def.Id);
            view.Display.SetRotationEnabled(false);

            if (poseLib != null && poseLib.TryGetUpgradePose(def.Class, out var pose))
                view.Display.ApplyBasePose(pose.LocalPosition, pose.LocalEulerAngles);

            BuildList();
            SelectFirst();
            DrawInfoPreview();
        }

        public void Exit() { }
        public void Tick()
        {
            foreach (var it in items.Values) it.Tick();
        }

        private void BuildList()
        {
            bar.Clear();
            items.Clear();

            foreach (var part in def.Parts)
            {
                var v = bar.CreateItem();
                var p = new UpgradeOptionItemPresenter(v, def, part, repo, upgrade, time);
                items[part.Id] = p;
                bar.Hook(v, part.Id);
                p.Refresh(part.Id == selectedPartId);
            }
        }

        private void SelectFirst()
        {
            if (!string.IsNullOrEmpty(selectedPartId) && items.ContainsKey(selectedPartId)) return;
            var first = def.Parts.FirstOrDefault();
            if (first != null) selectedPartId = first.Id;

            HighlightSelection();
            FocusPose();
        }

        private void OnSelectItem(string partId)
        {
            selectedPartId = partId;
            HighlightSelection();
            FocusPose();
            DrawInfoPreview();
        }

        private void HighlightSelection()
        {
            foreach (var kv in items) kv.Value.Refresh(kv.Key == selectedPartId);
        }

        private void FocusPose()
        {
    var part = def.Parts.FirstOrDefault(p => p.Id == selectedPartId);
    if (part == null) return;

    // Read the slot enum from your WeaponPartDef
    var slot = part.SlotType; // <- assuming your def exposes PartSlotType as 'SlotType'

    if (focusLib != null && focusLib.TryGet(def.Class, slot, out var e))
        view.Display.SmoothFocusPose(e.LocalPosition, e.LocalEulerAngles, 0.25f);
    else
        view.Display.NudgeForAttention(new Vector3(0f, 12f, 0f));
        }

        private void DrawInfoPreview()
        {
            var part = def.Parts.FirstOrDefault(p => p.Id == selectedPartId);
            if (part == null) return;

            view.Info.SetHeader(def.DisplayName, def.Class.ToString().ToUpperInvariant());

            var current = stats.Compute(def, inst);

            // Preview one level up (clone instance and simulate +1 level for the selected part)
            var clone = new WeaponInstance
            {
                WeaponId = inst.WeaponId,
                Owned = inst.Owned,
                Equipped = inst.Equipped,
                PartLevels = new System.Collections.Generic.Dictionary<string, int>(inst.PartLevels),
                MasteryTiers = new System.Collections.Generic.Dictionary<string, int>(inst.MasteryTiers)
            };
            clone.PartLevels.TryGetValue(part.Id, out var lv);
            if (lv < part.MaxLevel) clone.PartLevels[part.Id] = lv + 1;

            var after = stats.Compute(def, clone);

            // Base stats
            view.Info.SetStats(normalization, style, current, showComparison: false, equipped: current);
            // Overlay the green "upgrade preview" deltas
            view.Info.GetComponentInChildren<WeaponStatsPanelView>()?.SetUpgradePreview(normalization, style, current, after);

            // ----- Purchase UI for upgrade (COINS only) -----
            bool atMax = lv >= part.MaxLevel;

            // In your data this field is still named CashCost; you’re using it as Coins cost.
            int price = (!atMax && part.Levels != null && lv >= 0 && lv < part.Levels.Length)
                        ? part.Levels[lv].CashCost
                        : 0;

            bool visible = !atMax;
            bool affordable = visible && wallet.CanAfford(host.CoinsCurrencyType, price);

            // Show coins purchase area with label "Upgrade"
            view.Info.ConfigurePurchase(host.CoinsCurrencyType, price, visible, affordable, label: "Upgrade");
        }


        private void OnPurchaseUpgrade(CurrencyType currency, int price)
        {
            // Selected part
            var part = def.Parts.FirstOrDefault(p => p.Id == selectedPartId);
            if (part == null) return;

            // Load state & current level
            var state = repo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id)
                       ?? new WeaponInstance { WeaponId = def.Id, Owned = true };
            inst.PartLevels.TryGetValue(part.Id, out var level);

            // Already maxed?
            if (level >= part.MaxLevel) return;

            // Afford?
            if (!wallet.TrySpend(currency, price)) return;

            // === INSTANT UPGRADE: apply level immediately, no job ===
            inst.PartLevels[part.Id] = level + 1;

            // Ensure instance is persisted in the save
            var saved = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
            if (saved == null)
                state.Weapons.Add(inst);

            repo.Save(state);

            // Feedback + UI refresh
            //PlayUpgradeFeedback();   // shake/VFX/slider anim if you already have it
            RefreshAll();            // recompute stats, redraw right panel, list, etc.
        }

        private void RefreshAll()
        {
            var state = repo.Load();
            inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id) ?? inst;

            foreach (var kv in items) kv.Value.Refresh(kv.Key == selectedPartId);
            DrawInfoPreview();
        }
    }
}
