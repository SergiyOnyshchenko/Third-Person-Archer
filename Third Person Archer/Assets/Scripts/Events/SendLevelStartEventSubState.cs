using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class SendLevelStartEventSubState : SubState, IActorIniter
{
    private ActorController _actor;
    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Exit()
    {
        RuntimeMissionEventManager.SendGameStarted();

        if (DataManager.Instance.TryGetData(out MissionProgressData data))
        {
            //int level_number = data.Mission.ID;
            //YsoCorp.GameUtils.YCManager.instance.OnGameStarted(level_number);
        }

        base.Exit();
    }
}
