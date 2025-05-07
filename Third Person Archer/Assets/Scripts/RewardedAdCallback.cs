using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RewardedAdCallback : MonoBehaviour
{
    public UnityEvent OnRewardSuccess = new UnityEvent();
    public UnityEvent OnRewardFail = new UnityEvent();

    public void ShowRewarded()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool value) =>
        {
            if (value)
            {
                OnRewardSuccess?.Invoke();
            }
            else
            {
                OnRewardFail?.Invoke();
            }
        });
    }
}
