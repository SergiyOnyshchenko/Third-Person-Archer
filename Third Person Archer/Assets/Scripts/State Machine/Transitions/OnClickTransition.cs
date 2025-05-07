using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnClickTransition : StateTransition
{
    [SerializeField] private Button _button;

    public override void Enter()
    {
        base.Enter();
        _button.onClick.AddListener(DoTransition);
    }

    private void Update()
    {
        if (_button != null)
            return;

        if (Input.GetMouseButtonUp(0))
            DoTransition();
    }

    public override void Exit()
    {
        _button.onClick.RemoveListener(DoTransition);
        base.Exit();
    }
}
