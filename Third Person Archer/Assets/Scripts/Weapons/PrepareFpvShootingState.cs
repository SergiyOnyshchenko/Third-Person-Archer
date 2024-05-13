using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class PrepareFpvShootingState : CamerPovState
{
    public override void SetCamera()
    {
        _cameraPOV.SetFirstPerson();
    }
}
