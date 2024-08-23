using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class HostageSavedTransition : StateTransition, IActorIniter
{
    private HostageController _hostageController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out HostageController controller))
        {
            _hostageController = controller;
        }
    }

    private void Update()
    {
        if(_hostageController == null)
            return;

        if (_hostageController.IsSaved)
            DoTransition();
    }
}
