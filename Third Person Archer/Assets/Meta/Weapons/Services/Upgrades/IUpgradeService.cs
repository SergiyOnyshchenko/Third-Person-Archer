using System;

namespace Meta.Weapons
{
    public enum UpgradePayment
    {
        CashWithTimer,
        GoldInstant
    }

    public interface IUpgradeService
    {
        event Action<UpgradeJob> OnUpgradeStarted;
        event Action<UpgradeJob> OnUpgradeCompleted;

        /// <summary> Starts a part upgrade for a weapon. Returns null if validation fails. </summary>
        UpgradeJob StartUpgrade(string weaponId, string partId, UpgradePayment payment);

        /// <summary> Should be called on app start or periodically to finalize due jobs. </summary>
        void ProcessDueUpgrades();

        /// <summary> Attempts a Mastery tier upgrade (post-cap). Returns true on success. </summary>
        bool TryUpgradeMastery(string weaponId, string partId);

        UpgradeJob[] GetActiveJobs();
    }

    [Serializable]
    public class UpgradeJob
    {
        public string JobId;
        public string WeaponId;
        public string PartId;
        public int TargetLevel;
        public DateTime UtcFinishAt;
    }
}