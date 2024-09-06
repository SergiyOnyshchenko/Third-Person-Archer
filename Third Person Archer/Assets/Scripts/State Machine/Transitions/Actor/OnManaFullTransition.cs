using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class OnManaFullTransition : StateTransition, IActorIniter
{
    private Mana _mana;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out Mana mana))
        {
            _mana = mana;
        }
    }

    public void Update()
    {
        if(_mana == null)
            return;

        if (_mana.Ratio == 1f)
            DoTransition();
    }
}
