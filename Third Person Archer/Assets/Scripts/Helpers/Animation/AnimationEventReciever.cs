using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventReciever : MonoBehaviour
{
    [SerializeField] private AnimationEvent[] _events;
    public AnimationEvent[] Events { get => _events; }
    public UnityEvent<string> OnAnimationEvent = new UnityEvent<string>();

    public void InvokeAnimationEvent(string name)
    {
        foreach (AnimationEvent animationEvent in _events)
        {
            if (animationEvent.TryInvoke(name))
                break;
        }

        OnAnimationEvent?.Invoke(name);
    }
}
