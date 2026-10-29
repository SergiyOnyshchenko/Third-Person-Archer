public interface IMetaProgressWrite : IMetaProgressReadOnly
{
    void SelectZone(int index);
    void SelectMissionType(MissionType type);

    /// <summary>Whole campaign done: Campaign levels are played in Heroic mode.</summary>
    bool IsHeroicUnlocked { get; }
    void AdvanceHeroic(ZoneData zone);
}
