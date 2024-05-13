using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishShootingTrasnition : ShootingTrasnition
{
    public override bool CheckTransition()
    {
        return !_input.IsActive;
    }
}
