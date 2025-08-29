using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.Economy;

namespace Meta.Weapons
{
    [Serializable]
    public class UpgradeJobsState
    {
        public List<UpgradeJob> Jobs = new();
    }

    public interface IUpgradeJobsRepository
    {
        UpgradeJobsState Load();
        void Save(UpgradeJobsState state);
    }

    public class UpgradeJobsRepository : IUpgradeJobsRepository
    {
        private const string FileName = "WeaponUpgradeJobs.json";

        public UpgradeJobsState Load()
        {
            var loaded = SaveSystem.Load(FileName, new UpgradeJobsState());
            return loaded ?? new UpgradeJobsState();
        }

        public void Save(UpgradeJobsState state)
        {
            SaveSystem.Save(FileName, state);
        }
    }

    public class UpgradeService : IUpgradeService
    {
        public event Action<UpgradeJob> OnUpgradeStarted;
        public event Action<UpgradeJob> OnUpgradeCompleted;

        private readonly IWeaponRepository _weaponRepo;
        private readonly IUpgradeJobsRepository _jobsRepo;
        private readonly IWalletService _wallet;
        private readonly ITimeProvider _time;
        private readonly WeaponDef[] _allDefs;

        public UpgradeService(IWeaponRepository weaponRepo,
                              IUpgradeJobsRepository jobsRepo,
                              IWalletService wallet,
                              ITimeProvider timeProvider,
                              WeaponDef[] allDefs)
        {
            _weaponRepo = weaponRepo;
            _jobsRepo = jobsRepo;
            _wallet = wallet;
            _time = timeProvider;
            _allDefs = allDefs;
        }

        public UpgradeJob[] GetActiveJobs() => _jobsRepo.Load().Jobs.ToArray();

        public UpgradeJob StartUpgrade(string weaponId, string partId, UpgradePayment payment)
        {
            var state = _weaponRepo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId && w.Owned);
            if (inst == null) return null;

            var def = _allDefs.FirstOrDefault(d => d.Id == weaponId);
            if (def == null) return null;

            var part = def.Parts.FirstOrDefault(p => p.Id == partId);
            if (part == null) return null;

            inst.PartLevels.TryGetValue(part.Id, out var currentLevel);
            if (currentLevel >= part.MaxLevel) return null; // Already capped (use Mastery instead)

            var nextIdx = Mathf.Clamp(currentLevel + 1, 1, part.MaxLevel) - 1;
            var levelDef = part.Levels[nextIdx];

            if (payment == UpgradePayment.GoldInstant)
            {
                if (!_wallet.CanAfford(CurrencyType.Gold, levelDef.GoldCost))
                    return null;

                _wallet.Spend(CurrencyType.Gold, levelDef.GoldCost);
                inst.PartLevels[part.Id] = currentLevel + 1;
                _weaponRepo.Save(state);

                var instantJob = new UpgradeJob
                {
                    JobId = Guid.NewGuid().ToString("N"),
                    WeaponId = weaponId,
                    PartId = partId,
                    TargetLevel = currentLevel + 1,
                    UtcFinishAt = _time.UtcNow
                };
                OnUpgradeStarted?.Invoke(instantJob);
                OnUpgradeCompleted?.Invoke(instantJob);
                return instantJob;
            }
            else
            {
                if (!_wallet.CanAfford(CurrencyType.Cash, levelDef.CashCost))
                    return null;

                _wallet.Spend(CurrencyType.Cash, levelDef.CashCost);

                var jobsState = _jobsRepo.Load();
                var job = new UpgradeJob
                {
                    JobId = Guid.NewGuid().ToString("N"),
                    WeaponId = weaponId,
                    PartId = partId,
                    TargetLevel = currentLevel + 1,
                    UtcFinishAt = _time.UtcNow.AddSeconds(levelDef.BuildSeconds)
                };
                jobsState.Jobs.Add(job);
                _jobsRepo.Save(jobsState);

                OnUpgradeStarted?.Invoke(job);
                return job;
            }
        }

        public void ProcessDueUpgrades()
        {
            var jobs = _jobsRepo.Load();
            if (jobs.Jobs.Count == 0) return;

            var now = _time.UtcNow;
            var due = jobs.Jobs.Where(j => j.UtcFinishAt <= now).ToList();
            if (due.Count == 0) return;

            var state = _weaponRepo.Load();

            foreach (var job in due)
            {
                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == job.WeaponId && w.Owned);
                var def = _allDefs.FirstOrDefault(d => d.Id == job.WeaponId);
                if (inst == null || def == null) continue;

                var part = def.Parts.FirstOrDefault(p => p.Id == job.PartId);
                if (part == null) continue;

                // finalize level
                inst.PartLevels.TryGetValue(part.Id, out var curLevel);
                if (job.TargetLevel == curLevel + 1)
                {
                    inst.PartLevels[part.Id] = job.TargetLevel;
                }

                OnUpgradeCompleted?.Invoke(job);
                jobs.Jobs.Remove(job);
            }

            _weaponRepo.Save(state);
            _jobsRepo.Save(jobs);
        }

        public bool TryUpgradeMastery(string weaponId, string partId)
        {
            var state = _weaponRepo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId && w.Owned);
            if (inst == null) return false;

            var def = _allDefs.FirstOrDefault(d => d.Id == weaponId);
            var part = def?.Parts.FirstOrDefault(p => p.Id == partId);
            if (part == null || part.Mastery == null) return false;

            // Require part at max level
            inst.PartLevels.TryGetValue(part.Id, out var level);
            if (level < part.MaxLevel) return false;

            inst.MasteryTiers.TryGetValue(part.Id, out var currentTier);
            if (currentTier >= part.Mastery.MaxTier) return false;

            var nextTier = currentTier + 1;
            var tierDef = part.Mastery.Tiers[nextTier - 1];
            if (!_wallet.CanAfford(CurrencyType.KillTags, tierDef.KillTagsCost))
                return false;

            _wallet.Spend(CurrencyType.KillTags, tierDef.KillTagsCost);
            inst.MasteryTiers[part.Id] = nextTier;

            _weaponRepo.Save(state);
            return true;
        }
    }
}
