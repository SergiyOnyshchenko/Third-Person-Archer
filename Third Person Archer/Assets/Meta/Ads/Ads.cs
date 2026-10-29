namespace Meta.Ads
{
    /// <summary>
    /// Static facade for rewarded ads (same pattern as Meta.Economy.Economy).
    /// Falls back to <see cref="StubAdsService"/> until a real SDK adapter is installed.
    /// </summary>
    public static class Ads
    {
        private static IAdsService _service;

        public static IAdsService Service => _service ??= new StubAdsService();

        public static void Initialize(IAdsService service) => _service = service;
    }

    public static class AdPlacements
    {
        public const string VictoryMultiplier = "reward_multiplier";
        public const string WeaponUpgrade = "weapon_upgrade";
        public const string SkinUnlock = "skin_unlock";
        public const string GuildRefresh = "guild_refresh";
    }
}
