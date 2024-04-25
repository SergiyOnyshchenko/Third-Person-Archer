using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickTransition : StateTransition
{
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
            DoTransition();
    }
}
