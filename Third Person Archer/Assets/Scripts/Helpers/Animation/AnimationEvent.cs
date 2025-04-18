using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AnimationEvent 
{
    [field: SerializeField] public string Name { get; private set; }
    public UnityEvent Event = new UnityEvent();

    public bool TryInvoke(string name)
    {
        if (name != Name)
            return false;

        Event?.Invoke();
        return true;
    }
}
