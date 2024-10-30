using Firebase.Extensions;
using Firebase.Crashlytics;
using GameAnalyticsSDK;
using UnityEngine;

public class SDKManagement : MonoBehaviour, IGameAnalyticsATTListener
{
    void Start()
    {
        GameAnalyticsInit();
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                }
            }
        });
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
