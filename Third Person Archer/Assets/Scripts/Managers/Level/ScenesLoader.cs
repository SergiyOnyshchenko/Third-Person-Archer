using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class ScenesLoader : MonoBehaviour
{
    private MissionProgressData _missionProgressData;
    public static ScenesLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (DataManager.Instance.TryGetData(out _missionProgressData)) { }
    }

    public void LoadCurrentMission()
    {
        if (_missionProgressData == null)
        {
            Debug.LogError("[ScenesLoader] No Mission Progress Data.");
            return;
        }

        var mission = _missionProgressData.GetMission(_missionProgressData.MissionType);

        if (mission == null)
        {
            Debug.LogError("[ScenesLoader] No current mission set in MissionProgress.");
            return;
        }

        LoadScene(mission.Scene.ScenePath);
    }

    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    public void LoadMission(MissionData mission)
    {
        if (mission == null)
        {
            Debug.LogError("[ScenesLoader] LoadMission called with NULL mission.");
            return;
        }

        var scenePath = mission.Scene != null ? mission.Scene.ScenePath : null;
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError($"[ScenesLoader] Mission '{mission.Name}' has no scene path.");
            return;
        }

        LoadScene(scenePath);
    }

    public void LoadScene(string scenePath)
    {
        if (Preloader.Instance == null)
        {
            SceneManager.LoadScene(scenePath);
        }
        else
        {
            Preloader.Instance.FadeIn(() =>
            {
                SceneManager.LoadScene(scenePath);
            });
        }
    }
}