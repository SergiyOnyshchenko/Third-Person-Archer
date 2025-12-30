using System;
using UnityEngine;
using Meta.Weapons;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class RewardModule
    {
        [Header("Reward Profiles")]
        [Tooltip("Rewards by mission type. Each profile can define Cash / MissionToken / AllTokens progressions.")]
        [SerializeField] private MissionRewardProfile[] _profiles;

        public MissionRewardResult GetMissionReward(
            MissionType missionType,
            int index,
            WeaponClass missionWeaponClass,
            int weaponClassCount,
            RoundingModule rounding,
            int loopDeterminismSalt = 0)
        {
            int cash = 0;
            var tokens = new int[Mathf.Max(weaponClassCount, 0)];

            var profile = FindProfile(missionType);
            if (profile == null || profile.Entries == null)
                return new MissionRewardResult(0, tokens);

            for (int i = 0; i < profile.Entries.Length; i++)
            {
                var entry = profile.Entries[i];
                if (entry == null || entry.Progression == null)
                    continue;

                int key = BuildDeterminismKey(missionType, entry.Channel, index, (int)missionWeaponClass, loopDeterminismSalt);

                float raw = entry.Progression.Evaluate(index, key);

                switch (entry.Channel)
                {
                    case RewardChannel.Cash:
                    {
                        cash += rounding.RoundMoneyToNiceInt(raw);
                        break;
                    }

                    case RewardChannel.MissionToken:
                    {
                        int t = Mathf.RoundToInt(rounding.RoundTokensToNice(raw));
                        int wi = (int)missionWeaponClass;
                        if (wi >= 0 && wi < tokens.Length)
                            tokens[wi] += Mathf.Max(0, t);
                        break;
                    }

                    case RewardChannel.AllTokens:
                    {
                        int t = Mathf.RoundToInt(rounding.RoundTokensToNice(raw));
                        t = Mathf.Max(0, t);
                        for (int w = 0; w < tokens.Length; w++)
                            tokens[w] += t;
                        break;
                    }
                }
            }

            return new MissionRewardResult(cash, tokens);
        }

        private MissionRewardProfile FindProfile(MissionType missionType)
        {
            if (_profiles == null) return null;

            for (int i = 0; i < _profiles.Length; i++)
            {
                var p = _profiles[i];
                if (p != null && p.MissionType == missionType)
                    return p;
            }

            return null;
        }

        private static int BuildDeterminismKey(MissionType type, RewardChannel channel, int index, int weaponClass, int salt)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + (int)type;
                h = h * 31 + (int)channel;
                h = h * 31 + index;
                h = h * 31 + weaponClass;
                h = h * 31 + salt;
                return h;
            }
        }
    }
}