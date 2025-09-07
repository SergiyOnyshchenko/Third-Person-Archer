using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMainMenuSubState : SubState
{
    public override void Enter()
    {
        base.Enter();

        ScenesLoader.Instance.LoadMainMenu();
    }
}
