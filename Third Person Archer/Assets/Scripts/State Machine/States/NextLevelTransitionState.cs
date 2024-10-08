using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelTransitionState : MainState
{
    public override void Enter()
    {
        base.Enter();

        LevelEventSystem.SendLoadNextLevel();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
