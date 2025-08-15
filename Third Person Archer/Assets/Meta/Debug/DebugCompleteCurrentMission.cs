using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCompleteCurrentMission : MonoBehaviour
{
    public void CompleteCurrentMission()
    {
        if (DataManager.Instance.TryGetData(out MissionProgressData data))
            data.CompleteCurrentMission();
    }
}
