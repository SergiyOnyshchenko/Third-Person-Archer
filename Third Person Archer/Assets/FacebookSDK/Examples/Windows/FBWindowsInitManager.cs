using UnityEngine;
using Facebook.Unity;

public class FBWindowsInitManager : MonoBehaviour
{
    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (!FB.IsInitialized)
        {
            Debug.Log("GRE844IUVBA983>> [FBWindowsInitManager.Init] FB.IsInitialized == false, start initialization {Facebook}");
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            Debug.Log("GRE844IUVBA983>> [FBWindowsInitManager.Init] FB.IsInitialized == true, start activation {Facebook}");
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            Debug.Log("SDK correctly initializated: " + FB.FacebookImpl.SDKUserAgent);

            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
