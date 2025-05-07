using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class ReviveTransition : StateTransition
{
    public override void Enter()
    {
        base.Enter();
        LevelEventSystem.OnContinueLevel.AddListener(DoTransition);
    }

    public override void Exit() 
    {
        LevelEventSystem.OnContinueLevel.RemoveListener(DoTransition);
        base.Exit();
    }
}
