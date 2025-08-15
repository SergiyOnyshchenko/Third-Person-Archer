using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevelTransitionState : MainState
{
    public override void Enter()
    {
        base.Enter();

        ScenesLoader.Instance.LoadMainMenu();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
