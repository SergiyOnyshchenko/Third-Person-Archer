using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Sequence : MonoBehaviour
{
    [SerializeField] protected Sequence _nextSequence;
    public Sequence NextSequence { get => _nextSequence; }

    public UnityEvent OnBegin = new UnityEvent();
    public UnityEvent OnFinish = new UnityEvent();

    public virtual void Begin()
    {
        OnBegin?.Invoke();
    }

    protected virtual void Finish()
    {
        OnFinish?.Invoke();
    }

    public void Init(Sequence nextSequence)
    {
        _nextSequence = nextSequence;
    }
}
