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
        foreach (var animationEvent in _events)
        {
            animationEvent.TryInvoke(name);
        }

        OnAnimationEvent?.Invoke(name);
    }
}
