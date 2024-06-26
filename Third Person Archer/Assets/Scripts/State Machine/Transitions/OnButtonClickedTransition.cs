using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonClickedTransition : StateTransition
{
    [SerializeField] private Button _button;

    public override void Enter()
    {
        base.Enter();
        _button.onClick.AddListener(DoTransition);
    }

    public override void Exit() 
    {
        _button.onClick.RemoveListener(DoTransition);
        base.Exit();
    }
}
