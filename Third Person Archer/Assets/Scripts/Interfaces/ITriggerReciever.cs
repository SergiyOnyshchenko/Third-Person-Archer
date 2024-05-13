using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerReciever
{
    public event Action<string, GameObject> OnTriggered;
    public void ReciveTrigger(string name, GameObject owner);
}
