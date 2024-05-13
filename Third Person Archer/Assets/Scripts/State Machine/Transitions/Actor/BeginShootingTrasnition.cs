using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class BeginShootingTrasnition : ShootingTrasnition
{
    public override bool CheckTransition()
    {
        return _input.IsActive;
    }
}
