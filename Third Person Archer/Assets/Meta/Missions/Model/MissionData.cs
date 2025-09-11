using UnityEngine;

[CreateAssetMenu(fileName = "Mission", menuName = "GameMeta/MissionData", order = 1)]
public class MissionData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [Space]
    [SerializeField] private MissionType _missionType;
    [SerializeField] private UnlockCondition _unlockCondition;
    [Space]
    [SerializeField] private SceneReference _scene;

    public string ID { get => _id; }
    public string Name { get => _name; }
    public MissionType MissionType => _missionType;
    public SceneReference Scene { get => _scene; }
    public UnlockCondition UnlockCondition { get => _unlockCondition; }
    public bool HasUnlockCondition() =>
    UnlockCondition != null && UnlockCondition.Requirements != null && UnlockCondition.Requirements.Count > 0;
}
