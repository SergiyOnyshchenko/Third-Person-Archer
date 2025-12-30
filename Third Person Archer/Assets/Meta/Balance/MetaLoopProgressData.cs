using UnityEngine;

[CreateAssetMenu(fileName = "MetaLoopProgressData", menuName = "GameMeta/MetaLoopProgressData")]
public class MetaLoopProgressData : GameData
{
    [SerializeField] private int _currentLoopIndex = 0;
    [SerializeField] private int _completedCampaignLoops = 0;

    [Header("Grind indices")]
    [SerializeField] private int _contractsCompletedIndex = 0;
    [SerializeField] private int _sniperCompletedIndex = 0;

    public int CurrentLoopIndex => _currentLoopIndex;
    public int CompletedCampaignLoops => _completedCampaignLoops;

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

    public void OnFullCampaignAndAllBossesCompleted()
    {
        _completedCampaignLoops++;
        _currentLoopIndex++;
        Save();
    }

    public override void Initialize()
    {
        LoadPersistentData();
    }

    public void LoadPersistentData()
    {
        _currentLoopIndex = SaveSystem.Load("meta_loop_current_loop", 0);
        _completedCampaignLoops = SaveSystem.Load("meta_loop_completed_loops", 0);
        _contractsCompletedIndex = SaveSystem.Load("meta_loop_contracts_completed", 0);
        _sniperCompletedIndex = SaveSystem.Load("meta_loop_sniper_completed", 0);
    }

    public void Save()
    {
        SaveSystem.Save("meta_loop_current_loop", _currentLoopIndex);
        SaveSystem.Save("meta_loop_completed_loops", _completedCampaignLoops);
        SaveSystem.Save("meta_loop_contracts_completed", _contractsCompletedIndex);
        SaveSystem.Save("meta_loop_sniper_completed", _sniperCompletedIndex);
    }
}