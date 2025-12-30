using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNextLevelSubState : SubState
{
    public override void Enter()
    {
        base.Enter();

        if (DataManager.Instance.TryGetData(out MissionProgressData data))
        {
            //data.CompleteCurrentMission();
        }
    }
}
