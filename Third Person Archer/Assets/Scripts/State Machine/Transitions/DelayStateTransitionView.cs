using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayStateTransitionView : MonoBehaviour
{
    [SerializeField] private DelayTransition _delayTransition;

    private void Awake()
    {
        if(_delayTransition == null)
            _delayTransition = GetComponent<DelayTransition>();
    }

    private void OnEnable()
    {
        _delayTransition.OnEnter.AddListener(ShowTimerView);
        _delayTransition.OnExit.AddListener(HideTimerView);
    }

    private void OnDisable()
    {
        _delayTransition.OnEnter.RemoveListener(ShowTimerView);
        _delayTransition.OnExit.RemoveListener(HideTimerView);
    }

    private void ShowTimerView()
    {

    }

    private void HideTimerView()
    {

    }
}
