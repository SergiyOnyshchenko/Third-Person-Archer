using UnityEngine;
using UnityEngine.Events;

public abstract class PopupView : MonoBehaviour
{
    public UnityAction OnCloseRequested;

    public abstract void Initialize(string message);
    public abstract float GetDuration();

    public virtual void Close()
    {
        OnCloseRequested?.Invoke();
    }
}