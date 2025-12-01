using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ActorNoiseReceiver : MonoBehaviour, IActorIniter, ISoundListener
{
    private ActionInput _actions;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out _actions)){}
    }

    public void ReciveSound(string name, float power, GameObject owner)
    {
        _actions.DoAction("Alert");
    }
}
