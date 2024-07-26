using GameAnalyticsSDK;
using UnityEngine;

public class SDKManagement : MonoBehaviour, IGameAnalyticsATTListener
{
    void Start()
    {
        GameAnalyticsInit();
    }

    #region GameAnalytics

    private void GameAnalyticsInit()
    {
        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerRestricted()
    {
        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerDenied()
    {
        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerAuthorized()
    {
        GameAnalytics.SetCustomId(SystemInfo.deviceUniqueIdentifier);
        GameAnalytics.Initialize();
    }

    #endregion
}
