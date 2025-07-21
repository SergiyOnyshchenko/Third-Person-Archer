using UnityEngine;

[CreateAssetMenu(fileName = "Mission", menuName = "GameMeta/MissionData", order = 1)]
public class MissionData : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private string _name;
    [Space]
    [SerializeField] private MissionType _missionType;
    [Space]
    [SerializeField] private SceneReference _scene;

    private bool _isUnlocked;
    private bool _isCompleted;

    public int ID { get => _id; }
    public string Name { get => _name; }
    public MissionType MissionType => _missionType;
    public bool IsUnlocked => _isUnlocked;
    public bool IsCompleted => _isCompleted;
    public SceneReference Scene { get => _scene; }

    public void MarkCompleted()
    {
        _isCompleted = true;
    }

    public void Unlock()
    {
        _isUnlocked = true;
    }
}
