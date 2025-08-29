using System;
using System.Linq;
using Meta.Weapons.UI.Selection;

namespace Meta.Weapons.UI.Upgrade
{
    public class UpgradeOptionItemPresenter
    {
        private readonly UpgradeOptionItemView view;
        private readonly WeaponDef weaponDef;
        private readonly WeaponPartDef partDef;
        private readonly IWeaponRepository repository;
        private readonly IUpgradeService upgradeService;
        private readonly ITimeProvider timeProvider;

        public string PartId => partDef.Id;

        public UpgradeOptionItemPresenter(
            UpgradeOptionItemView view,
            WeaponDef weaponDef,
            WeaponPartDef partDef,
            IWeaponRepository repository,
            IUpgradeService upgradeService,
            ITimeProvider timeProvider)
        {
            this.view = view;
            this.weaponDef = weaponDef;
            this.partDef = partDef;
            this.repository = repository;
            this.upgradeService = upgradeService;
            this.timeProvider = timeProvider;
        }

        public void Refresh(bool selected)
        {
            var state = repository.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponDef.Id);
            inst ??= new WeaponInstance { WeaponId = weaponDef.Id };

            inst.PartLevels.TryGetValue(partDef.Id, out var level);
            bool atMax = level >= partDef.MaxLevel;

            view.SetBase(partDef.DisplayName, level, partDef.MaxLevel);
            view.SetSelected(selected);

            // Timer/progress
            var job = upgradeService.GetActiveJobs().FirstOrDefault(j => j.WeaponId == weaponDef.Id && j.PartId == partDef.Id);
            if (job != null)
            {
                var remaining = job.UtcFinishAt - timeProvider.UtcNow;
                view.SetTimer(Format(remaining), visible: true);
                view.SetBadges(inProgress: true, maxed: false);
            }
            else
            {
                view.SetTimer("", visible: false);
                view.SetBadges(inProgress: false, maxed: atMax);
            }
        }

        public void Tick()
        {
            var job = upgradeService.GetActiveJobs().FirstOrDefault(j => j.WeaponId == weaponDef.Id && j.PartId == partDef.Id);
            if (job == null)
            {
                view.SetTimer("", false);
                return;
            }
            var rem = job.UtcFinishAt - timeProvider.UtcNow;
            if (rem.TotalSeconds < 0)
                return; // parent presenter will refresh on UpgradeService.OnUpgradeCompleted
            view.SetTimer(Format(rem), true);
        }

        private static string Format(TimeSpan ts)
        {
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}