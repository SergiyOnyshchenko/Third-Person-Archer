using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public interface IShootingTargetsData 
{
    public void InitShootingTargets(ActorController[] targets, ActorController[] hostages);
}
