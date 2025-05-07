using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLevelStartSubstate : SubState
{
    public override void Exit()
    {
        LevelEventSystem.SendLevelStart();
        base.Exit();
    }
}
