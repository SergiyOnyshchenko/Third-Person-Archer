using System;
using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI.Upgrade;
using Meta.Weapons.UI.Display;
using Meta.Economy;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Orchestrates the unified shell and swaps Selection/Upgrade sub-presenters.
    /// Keeps shared state (selected weapon, etc).
    /// </summary>
    public sealed class WeaponScreenPresenter
    {
        private readonly WeaponScreenView view;
        private readonly SelectionBottomBarView selectionBarPrefab;
        private readonly UpgradeBottomBarView upgradeBarPrefab;

        // Shared services & data
        private readonly UIStatNormalizationConfig normalization;
        private readonly UIStyleConfig style;
        private readonly IWeaponRepository weaponRepository;
        private readonly IEquipmentService equipmentService;
        private readonly IStatsService statsService;
        private readonly IUpgradeService upgradeService;
        private readonly IWallet walletService;
        private readonly IWeaponIconProvider iconProvider;
        private readonly IWeaponPrefabProvider prefabProvider;
        private readonly WeaponDef[] catalog;
        private readonly ITimeProvider timeProvider;
        private readonly WeaponUpgradeFocusMap focusMap;
        private readonly WeaponDisplayPoseLibrary poseLibrary;
        private readonly WeaponPartFocusLibrary partFocusLibrary;
        private readonly Func<WeaponClass, bool> isClassUnlocked;

        // Mode machine
        private WeaponScreenMode mode;
        private IWeaponScreenModeController controller;

        // Shared state
        internal string SelectedWeaponId { get; set; } // persisted across modes
        internal CurrencyType CoinsCurrencyType { get; } = CurrencyType.Cash;

        public WeaponScreenPresenter(
            WeaponScreenView view,
            SelectionBottomBarView selectionBarPrefab,
            UpgradeBottomBarView upgradeBarPrefab,
            UIStatNormalizationConfig normalization,
            UIStyleConfig style,
            IWeaponRepository weaponRepository,
            IEquipmentService equipmentService,
            IStatsService statsService,
            IUpgradeService upgradeService,
            IWallet walletService,
            IWeaponIconProvider iconProvider,
            IWeaponPrefabProvider prefabProvider,
            WeaponDef[] catalog,
            ITimeProvider timeProvider,
            WeaponUpgradeFocusMap focusMap,
            WeaponDisplayPoseLibrary poseLibrary,
            WeaponPartFocusLibrary partFocusLibrary,
            Func<WeaponClass, bool> isClassUnlocked = null)  
        {
            this.view = view;
            this.selectionBarPrefab = selectionBarPrefab;
            this.upgradeBarPrefab = upgradeBarPrefab;
            this.normalization = normalization;
            this.style = style;
            this.weaponRepository = weaponRepository;
            this.equipmentService = equipmentService;
            this.statsService = statsService;
            this.upgradeService = upgradeService;
            this.walletService = walletService;
            this.iconProvider = iconProvider;
            this.prefabProvider = prefabProvider;
            this.catalog = catalog;
            this.timeProvider = timeProvider;
            this.focusMap = focusMap;
            this.poseLibrary = poseLibrary;
            this.partFocusLibrary = partFocusLibrary;
            this.isClassUnlocked = isClassUnlocked ?? (_ => true);

            this.view.Display.Initialize(prefabProvider);
            this.view.OnBack += () => SwitchMode(WeaponScreenMode.Selection);
        }

        public void Show()
        {
            SwitchMode(WeaponScreenMode.Selection);
        }

        public void Tick()
        {
            controller?.Tick();
        }

        public void SwitchMode(WeaponScreenMode next)
        {
            if (mode == next && controller != null) return;

            controller?.Exit();
            controller = null;

            view.SetBackVisible(next == WeaponScreenMode.Upgrade);
            view.Info.ClearContext();

            switch (next)
            {
                case WeaponScreenMode.Selection:
                    {
                        var bar = view.SpawnBottomPanel(selectionBarPrefab);
                        controller = new SelectionModePresenter(this, view, bar,
                            normalization, style, weaponRepository, equipmentService,
                            statsService, walletService, iconProvider, catalog, poseLibrary,
                            isClassUnlocked);
                        break;
                    }
                case WeaponScreenMode.Upgrade:
                    {
                        var bar = view.SpawnBottomPanel(upgradeBarPrefab);
                        controller = new UpgradeModePresenter(this, view, bar,
                            normalization, style, weaponRepository, upgradeService,
                            equipmentService, statsService, walletService, timeProvider,
                            catalog, focusMap, poseLibrary, partFocusLibrary);
                        break;
                    }
            }

            mode = next;
            controller.Enter();
        }
    }

    internal interface IWeaponScreenModeController
    {
        void Enter();
        void Exit();
        void Tick();
    }
}
