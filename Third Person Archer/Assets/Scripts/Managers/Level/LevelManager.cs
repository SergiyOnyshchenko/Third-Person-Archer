using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public static LevelManager Instance;

    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private bool _enableAd = true;

    public MissionData CurrentMission { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void LoadMainMenu() {
        LoadSceneWithFade(_mainMenuSceneName);
    }

    public void LaunchMission(MissionData missionData, bool useAd = false) {
        if (missionData == null || missionData.Scene == null) {
            Debug.LogError("Invalid mission or scene reference.");
            return;
        }

        CurrentMission = missionData;
        LoadSceneWithFade(missionData.Scene.ScenePath, useAd);
    }

    private void LoadSceneWithFade(string sceneName, bool useAd = false) {
        if (Preloader.Instance != null) {
            Preloader.Instance.FadeIn(() => {
                if (useAd && _enableAd) {
                    YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() => {
                        SceneManager.LoadScene(sceneName);
                    });
                } else {
                    SceneManager.LoadScene(sceneName);
                }
            });
        } else {
            SceneManager.LoadScene(sceneName);
        }
    }
}