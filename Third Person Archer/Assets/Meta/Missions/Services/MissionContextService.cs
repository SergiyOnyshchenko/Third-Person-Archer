using Meta.Weapons;

public sealed class MissionContextService : IMissionContextService
{
    private readonly IMetaProgressReadOnly _progress;
    private readonly IMissionCatalogService _catalog;
    private readonly IWeaponRequirementService _weaponRequirement;

    public MissionContextService(
        IMetaProgressReadOnly progress,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement)
    {
        _progress = progress;
        _catalog = catalog;
        _weaponRequirement = weaponRequirement;
    }

    public MissionContext BuildSelectedContext()
    {
        var zone = _progress.CurrentZone;
        int zoneIndex = GetZoneIndex(zone);

        var selectedType = _progress.SelectedMissionType;
        var mission = _progress.GetMission(selectedType);

        // Global campaign index is only guaranteed for actual campaign missions.
        // For Boss/Sniper/Contracts it can be -1 (and that’s OK).
        int globalCampaignIndex = _catalog.GetGlobalCampaignIndexOrMinusOne(mission);

        // Company level: if mission is in campaign list use it; otherwise infer from progress later (we’ll do that in availability service).
        int companyLevel = globalCampaignIndex >= 0 ? globalCampaignIndex + 1 : -1;

        WeaponClass requiredClass = _weaponRequirement.GetRequiredWeaponClass(mission);

        return new MissionContext(
            zone,
            zoneIndex,
            selectedType,
            mission,
            globalCampaignIndex,
            companyLevel,
            requiredClass
        );
    }

    private int GetZoneIndex(ZoneData zone)
    {
        var zones = _progress.AllZones;
        if (zones == null || zone == null)
            return 0;

        for (int i = 0; i < zones.Count; i++)
            if (zones[i] == zone)
                return i;

        return 0;
    }
}