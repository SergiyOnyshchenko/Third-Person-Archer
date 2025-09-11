using System;
using System.Linq;
using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI.Upgrade;
using Meta.Economy;
using Meta.Weapons.UI.Display;

namespace Meta.Weapons.UI
{
    internal sealed class SelectionModePresenter : IWeaponScreenModeController
    {
        private readonly WeaponScreenPresenter host;
        private readonly WeaponScreenView view;
        private readonly SelectionBottomBarView bar;

        private readonly UIStatNormalizationConfig normalization;
        private readonly UIStyleConfig style;
        private readonly IWeaponRepository repo;
        private readonly IEquipmentService equip;
        private readonly IStatsService stats;
        private readonly IWallet wallet;
        private readonly IWeaponIconProvider iconProvider;
        private readonly WeaponDef[] catalog;
        private readonly WeaponDisplayPoseLibrary poseLib;
        private readonly Func<WeaponClass, bool> isClassUnlocked;
        private WeaponClass activeClass;
        private bool _isActive;
    
        public SelectionModePresenter(
            WeaponScreenPresenter host,
            WeaponScreenView view,
            SelectionBottomBarView bar,
            UIStatNormalizationConfig normalization,
            UIStyleConfig style,
            IWeaponRepository repo,
            IEquipmentService equip,
            IStatsService stats,
            IWallet wallet,
            IWeaponIconProvider iconProvider,
            WeaponDef[] catalog,
            WeaponDisplayPoseLibrary poseLib,
            Func<WeaponClass, bool> isClassUnlocked = null)
        {
            this.host = host;
            this.view = view;
            this.bar = bar;
            this.normalization = normalization;
            this.style = style;
            this.repo = repo;
            this.equip = equip;
            this.stats = stats;
            this.wallet = wallet;
            this.iconProvider = iconProvider;
            this.catalog = catalog;
            this.poseLib = poseLib;
            this.isClassUnlocked = isClassUnlocked ?? (_ => true);

            bar.OnTabSelected += OnTab;
            bar.OnWeaponSelected += OnWeaponSelected;

            view.Info.OnEquip = OnEquip;
            view.Info.OnUpgrade = OnUpgrade; // navigate to upgrade when selected == equipped
            view.Info.OnPurchase = OnPurchaseWeapon;
        }

        public void Enter()
        {
            _isActive = true;
            view.Display.SetRotationEnabled(true);

            var unlockedClasses = catalog.
                Select(d => d.Class).
                Distinct().Where(c => this.isClassUnlocked(c)).
                OrderBy(c => c.ToString()).ToList();

            activeClass = unlockedClasses.FirstOrDefault();

            RebuildTabs();
            RebuildList();
            SelectInitial();
        }

        public void Exit()
        {
            _isActive = false;
        }

        public void Tick() { }

        private void RebuildTabs()
        {
            bar.ClearTabs();
            foreach (var cls in catalog.Select(d => d.Class)
                                       .Distinct()
                                       .OrderBy(c => c.ToString()))
            {
                if (!isClassUnlocked(cls)) continue;                                   // <— NEW: skip locked

                var t = bar.CreateTab();
                t.Set(cls.ToString().ToUpperInvariant(), isSelected: cls.Equals(activeClass));
                bar.HookTab(t, cls);
            }
        }

        private void RebuildList()
        {
            if (!_isActive) return;
            if (bar == null || bar.Equals(null)) return; 

            bar.ClearWeaponList();
            var state = repo.Load();

            foreach (var def in catalog.Where(d => d.Class == activeClass))
            {
                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
                bool owned = inst?.Owned == true;
                bool equipped = inst?.Equipped == true;

                var item = bar.CreateItem();
                item.Bind(iconProvider.GetIcon(def.Id), def.DisplayName, owned, equipped);
                bar.HookItem(item, def.Id);
            }
        }

        private void SelectInitial()
        {
            var st = repo.Load();
            // Prefer equipped in this class
            var eq = st.Weapons.FirstOrDefault(w =>
            {
                if (!w.Equipped) return false;
                var d = catalog.FirstOrDefault(x => x.Id == w.WeaponId);
                return d != null && d.Class == activeClass;
            });

            if (eq != null) { OnWeaponSelected(eq.WeaponId); return; }

            var owned = st.Weapons.FirstOrDefault(w =>
            {
                if (!w.Owned) return false;
                var d = catalog.FirstOrDefault(x => x.Id == w.WeaponId);
                return d != null && d.Class == activeClass;
            });

            if (owned != null) { OnWeaponSelected(owned.WeaponId); return; }

            var first = catalog.FirstOrDefault(d => d.Class == activeClass);
            if (first != null) OnWeaponSelected(first.Id);
        }

        private void OnTab(WeaponClass cls)
        {
            activeClass = cls;
            RebuildTabs();
            RebuildList();
            SelectInitial();
        }

        private void OnWeaponSelected(string weaponId)
        {
            host.SelectedWeaponId = weaponId;

            var def = catalog.First(d => d.Id == weaponId);

            view.Display.LoadWeaponModel(weaponId);
            view.Display.SetRotationEnabled(true);

            // Apply class-based selection pose
            if (poseLib != null && poseLib.TryGetSelectionPose(def.Class, out var pose))
                view.Display.ApplyBasePose(pose.LocalPosition, pose.LocalEulerAngles);

            DrawInfo();

        }

        private void DrawInfo()
        {
            var def = catalog.First(d => d.Id == host.SelectedWeaponId);
            var st = repo.Load();

            var selInst = st.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
            var selStats = stats.Compute(def, selInst ?? new WeaponInstance { WeaponId = def.Id });

            var eqInst = st.Weapons.FirstOrDefault(w => w.Equipped);
            WeaponStats eqStats;
            bool showComparison = false;
            if (eqInst != null)
            {
                var eqDef = catalog.FirstOrDefault(d => d.Id == eqInst.WeaponId);
                if (eqDef != null && eqDef.Class == def.Class && eqInst.WeaponId != def.Id)
                {
                    eqStats = stats.Compute(eqDef, eqInst);
                    showComparison = true;
                }
                else eqStats = selStats;
            }
            else eqStats = selStats;

            bool owned = selInst?.Owned == true;
            bool isEquipped = selInst?.Equipped == true;

            view.Info.SetHeader(def.DisplayName, def.Class.ToString().ToUpperInvariant());
            view.Info.SetStats(normalization, style, selStats, showComparison, eqStats);

            // Context:
            view.Info.ShowContextOwned(owned, isEquipped, selectedIsEquipped: isEquipped);

            // Coins purchase (only when not owned)
            if (!owned)
            {
                int price = def.Acquisition.CashPrice;
                bool visible = def.Acquisition.CanBuyWithCash;
                bool affordable = visible && wallet.CanAfford(host.CoinsCurrencyType, price);
                view.Info.ConfigurePurchase(host.CoinsCurrencyType, price, visible, affordable, "Get Now");
            }
        }

        private void OnEquip()
        {
            if (string.IsNullOrEmpty(host.SelectedWeaponId)) return;
            if (equip.Equip(host.SelectedWeaponId))
            {
                RebuildList();
                DrawInfo();
            }
        }

        private void OnUpgrade()
        {
            // Only allowed when selected == equipped; the view already hides the button otherwise.
            host.SwitchMode(WeaponScreenMode.Upgrade);
        }

        private void OnPurchaseWeapon(CurrencyType currency, int price)
        {
            if (!_isActive) return;

            var def = catalog.First(d => d.Id == host.SelectedWeaponId);
            if (!wallet.CanAfford(currency, price)) return;

            wallet.Spend(currency, price);

            var state = repo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
            if (inst == null)
            {
                inst = new WeaponInstance { WeaponId = def.Id, Owned = true, Equipped = false };
                state.Weapons.Add(inst);
            }
            else inst.Owned = true;
            repo.Save(state);

            RebuildList();
            DrawInfo();
        }
    }
}
