using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class Stage : Input
    {
        [SerializeField] private int _stageIndex;
        public int StageIndex { get => _stageIndex; }
        public UnityEvent<int> OnStageStarted = new UnityEvent<int>();
        public UnityEvent OnStageChanged = new UnityEvent();

        public void SetStage(int stageIndex)
        {
            _stageIndex = stageIndex;
            OnStageStarted?.Invoke(stageIndex);
            OnStageChanged?.Invoke();
        }

        public void IncreaseStage()
        {
            _stageIndex++;
            OnStageStarted?.Invoke(_stageIndex);
            OnStageChanged?.Invoke();
        }
    }
}

