using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class DelayStateTransitionView : MonoBehaviour
{
    [SerializeField] private TimerView _timerView;
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
        _timerView.Show();
    }

    private void HideTimerView()
    {
        _timerView.Hide();
    }

    private void Update()
    {
        if (!_delayTransition.enabled)
            return;

        _timerView.UpdateBar(_delayTransition.Ratio);
    }
}
