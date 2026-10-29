using UnityEngine;

[CreateAssetMenu(fileName = "MetaLoopProgressData", menuName = "GameMeta/MetaLoopProgressData")]
/// <summary>
/// Grind counters (contracts / sniper runs). Campaign loops were replaced by Heroic mode (MissionProgressData).
/// </summary>
public class MetaLoopProgressData : GameData
{
    [Header("Grind indices")]
    [SerializeField] private int _contractsCompletedIndex = 0;
    [SerializeField] private int _sniperCompletedIndex = 0;

    public int ContractsCompletedIndex => _contractsCompletedIndex;
    public int SniperCompletedIndex => _sniperCompletedIndex;

    public void IncreaseContractsCompleted()
    {
        _contractsCompletedIndex++;
        Save();
    }

    public void IncreaseSniperCompleted()
    {
        _sniperCompletedIndex++;
        Save();
    }

    public override void Initialize()
    {
        LoadPersistentData();
    }

    public void LoadPersistentData()
    {
        _contractsCompletedIndex = SaveSystem.Load("meta_loop_contracts_completed", 0);
        _sniperCompletedIndex = SaveSystem.Load("meta_loop_sniper_completed", 0);
    }

    public void Save()
    {
        SaveSystem.Save("meta_loop_contracts_completed", _contractsCompletedIndex);
        SaveSystem.Save("meta_loop_sniper_completed", _sniperCompletedIndex);
    }
}