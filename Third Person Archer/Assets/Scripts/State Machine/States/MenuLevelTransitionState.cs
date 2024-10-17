using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevelTransitionState : MainState
{
    public override void Enter()
    {
        base.Enter();

        LevelEventSystem.SendLoadMainMenu();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
