using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    [SerializeField] private Sequence _currentSequence;
    [SerializeField] private bool _beginOnAwake;

    private void Awake()
    {
        InitSequences();

        if (_beginOnAwake)
            Begin();
    }

    public void Begin()
    {
        BeginSequence(GetStartSequence());
    }
    
    private void InitSequences()
    {
        var sequences = GetComponentsInChildren<Sequence>();

        for (int i = 0; i < sequences.Length; i++)
        {
            if (i == sequences.Length - 1)
                continue;

            sequences[i].Init(sequences[i + 1]);
        }
    }

    private void BeginSequence(Sequence sequence)
    {
        if(_currentSequence != null)
        {
            _currentSequence.OnFinish.RemoveListener(SetNextSequence);
            _currentSequence.enabled = false;
        }
           
        _currentSequence = sequence;

        _currentSequence.OnFinish.AddListener(SetNextSequence);
        _currentSequence.Begin();
        _currentSequence.enabled = true;
    }

    private void SetNextSequence()
    {
        var nextSequence = _currentSequence.NextSequence;

        if (nextSequence == null)
            return;

        BeginSequence(nextSequence);
    }

    private Sequence GetStartSequence()
    {
        return GetComponentInChildren<Sequence>();
    }
}
