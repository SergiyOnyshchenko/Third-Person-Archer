using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFtvSubstate : CamerPovState
{
    public override void SetCamera()
    {
        _cameraPOV.SetThirdPerson();
    }
}
