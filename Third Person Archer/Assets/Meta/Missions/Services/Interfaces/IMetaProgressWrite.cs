public interface IMetaProgressWrite : IMetaProgressReadOnly
{
    void SelectZone(int index);
    void SelectMissionType(MissionType type);
}