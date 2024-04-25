using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReciever : MonoBehaviour
{
    [SerializeField] private AnimationEvent[] _events;

    public void InvokeAnimationEvent(string name)
    {
        foreach (var animationEvent in _events)
        {
            animationEvent.TryInvoke(name);
        }
    }
}
