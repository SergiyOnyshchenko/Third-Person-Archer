using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNextLevelSubState : SubState
{
    public override void Enter()
    {
        base.Enter();
        LevelManager.Instance.Database.SetNextLevel();
    }
}
