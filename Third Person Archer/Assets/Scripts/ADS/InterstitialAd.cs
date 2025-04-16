using UnityEngine;
using UnityEngine.Events;
using System;
using Playgama;
using Playgama.Modules.Advertisement;


public class InterstitialAd : MonoBehaviour
{
    public static InterstitialAd Instance;

    private UnityAction _onAdClosedCallback;

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

    private void Start()
    {
        Bridge.advertisement.SetMinimumDelayBetweenInterstitial(30);
    }

    public void ShowAd(UnityAction onAdClosed)
    {
        if (Bridge.advertisement.interstitialState == InterstitialState.Closed ||
            Bridge.advertisement.interstitialState == InterstitialState.Failed)
        {
            _onAdClosedCallback = onAdClosed;
            Bridge.advertisement.ShowInterstitial();
            Bridge.advertisement.interstitialStateChanged += OnInterstitialFinished;
        }
        else
        {
            Debug.Log("Interstitial ad not ready or already open.");
            onAdClosed?.Invoke();
        }
    }

    private void OnInterstitialFinished(InterstitialState state)
    {
        Bridge.advertisement.interstitialStateChanged -= OnInterstitialFinished;

        if (state != InterstitialState.Opened)
        {
            _onAdClosedCallback?.Invoke();
        }
    }
}
