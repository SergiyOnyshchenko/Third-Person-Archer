using System;
using UnityEngine;

namespace Meta.Weapons
{
    [Serializable]
    public class MissionRequirement
    {
        public WeaponClass RequiredClass;
        [Tooltip("If a stat is negative (e.g., ReloadTime lower is better), set Recommended as the 'max acceptable'.")]
        public WeaponStats RecommendedMinimum; // treat ReloadTime specially

        public bool ReloadTimeIsMaxNotMin = true; // lower reload time is better
    }

    public class MissionGateResult
    {
        public GateStatus Status;
        public string EquippedWeaponId; // null if none equipped for required class
        public string[] SuggestedUpgradesPartIds; // parts that most affect the weak stats
        public string[] SuggestedAlternativeWeaponIds; // owned better-suited options (if any)
    }

    public interface IMissionGateService
    {
        MissionGateResult Evaluate(MissionRequirement requirement);
    }
}

