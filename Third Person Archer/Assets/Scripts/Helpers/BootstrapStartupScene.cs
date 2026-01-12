using System;
using UnityEngine;

public class BootstrapStartupScene : MonoBehaviour
{
    [Header("Startup Scenes")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private string _firstTimeSceneName = "Cutscene1";

    [Header("Save")]
    [SerializeField] private string _saveFileName = "boot_state"; // SaveSystem file name
    [SerializeField] private bool _markAsPlayedBeforeLoading = true; // safer against crash loops

    private void Start()
    {
        if (ScenesLoader.Instance == null)
        {
            Debug.LogError("[BootstrapStartupSceneDecider] ScenesLoader.Instance is null. Put ScenesLoader in Bootstrap scene.");
            return;
        }

        var state = LoadState();

        // First time -> Cutscene1 (only once)
        if (!state.Cutscene1Played)
        {
            Debug.Log("[BootstrapStartupSceneDecider] First launch detected -> loading Cutscene1.");

            if (_markAsPlayedBeforeLoading)
            {
                state.Cutscene1Played = true;
                SaveState(state);
            }

            ScenesLoader.Instance.LoadScene(_firstTimeSceneName);

            if (!_markAsPlayedBeforeLoading)
            {
                state.Cutscene1Played = true;
                SaveState(state);
            }

            return;
        }

        // Not first time -> MainMenu
        Debug.Log("[BootstrapStartupSceneDecider] Not first launch -> loading MainMenu.");
        ScenesLoader.Instance.LoadScene(_mainMenuSceneName);
    }

#if UNITY_EDITOR
    [ContextMenu("DEBUG/Reset First Launch Flag")]
    private void DebugResetFirstLaunchFlag()
    {
        SaveState(new BootState { Cutscene1Played = false });
        Debug.Log("[BootstrapStartupSceneDecider] BootState reset (Cutscene1 will play on next launch).");
    }
#endif

    // -----------------------
    // Save payload
    // -----------------------
    [Serializable]
    private struct BootState
    {
        public bool Cutscene1Played;
    }

    private BootState LoadState()
    {
        try
        {
            // Your project’s SaveSystem API (you asked to use it)
            return SaveSystem.Load(_saveFileName, new BootState { Cutscene1Played = false });
        }
        catch (Exception e)
        {
            // Fallback if SaveSystem isn’t ready yet for some reason
            Debug.LogWarning($"[BootstrapStartupSceneDecider] SaveSystem load failed, fallback to PlayerPrefs. {e.Message}");

            return new BootState
            {
                Cutscene1Played = PlayerPrefs.GetInt("Cutscene1Played", 0) == 1
            };
        }
    }

    private void SaveState(BootState state)
    {
        try
        {
            SaveSystem.Save(_saveFileName, state);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BootstrapStartupSceneDecider] SaveSystem save failed, fallback to PlayerPrefs. {e.Message}");

            PlayerPrefs.SetInt("Cutscene1Played", state.Cutscene1Played ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
