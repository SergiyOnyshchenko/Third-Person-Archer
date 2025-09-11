using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionContext : MonoBehaviour
{
    public MissionData MissionData { get; private set; }
    public static MissionContext Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    private void Start()
    {
        if (DataManager.Instance.TryGetData(out MissionProgressData data))
        {
            MissionData = data.GetMission(data.MissionType);
        }
    }
}
