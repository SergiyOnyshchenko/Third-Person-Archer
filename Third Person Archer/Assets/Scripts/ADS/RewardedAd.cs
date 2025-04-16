using UnityEngine;
using UnityEngine.Events;
using System;
using Playgama;
using Playgama.Modules.Advertisement;

public class RewardedAd : MonoBehaviour
{
    public static RewardedAd Instance;

    public UnityEvent OnRewarded;
    public UnityEvent OnAdOpened;
    public UnityEvent OnAdClosed;

    private Action _onRewardedCallback;
    private Action _onAdClosedCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowAd(Action onRewarded, Action onAdClosed = null)
    {
        if (Bridge.advertisement.rewardedState == RewardedState.Closed || 
            Bridge.advertisement.rewardedState == RewardedState.Failed)
        {
            _onRewardedCallback = onRewarded;
            _onAdClosedCallback = onAdClosed;
            Bridge.advertisement.ShowRewarded();
            Bridge.advertisement.rewardedStateChanged += OnRewardedFinished;
        }
        else
        {
            Debug.Log("Rewarded ad not available.");
            onAdClosed?.Invoke();
        }
    }

    private void OnRewardedFinished(RewardedState state)
    {
        Bridge.advertisement.rewardedStateChanged -= OnRewardedFinished;

        switch (state)
        {
            case RewardedState.Opened:
                OnAdOpened?.Invoke();
                AudioListener.volume = 0;
                break;

            case RewardedState.Closed:
                OnAdClosed?.Invoke();
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                AudioListener.volume = 1;
                break;

            case RewardedState.Rewarded:
                OnRewarded?.Invoke();
                _onRewardedCallback?.Invoke();
                _onRewardedCallback = null;
                break;

            case RewardedState.Failed:
                OnAdClosed?.Invoke();
                _onAdClosedCallback?.Invoke();
                _onAdClosedCallback = null;
                AudioListener.volume = 1;
                break;
        }
    }
}

