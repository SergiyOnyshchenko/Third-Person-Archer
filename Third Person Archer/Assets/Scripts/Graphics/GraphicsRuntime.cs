using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies the look to every loaded scene and watches the frame rate at the start of each level:
/// if the device can't hold it on the detected level, the next level loads one step lower.
/// </summary>
public class GraphicsRuntime : MonoBehaviour
{
    private const float WarmUp = 4f;
    private const float SampleDuration = 12f;
    private const float MinFps = 48f;
    // Frames longer than this are loading hitches, not the steady frame rate.
    private const float HitchTime = 0.25f;

    private float _warmUp;
    private float _sampledTime;
    private int _sampledFrames;
    private bool _sampling;
    private Scene _appliedScene;

    public static void Create()
    {
        var go = new GameObject("Graphics Runtime");
        go.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(go);
        go.AddComponent<GraphicsRuntime>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // The scene open when the game starts may have loaded before we subscribed (always so in the editor).
        Scene active = SceneManager.GetActiveScene();
        if (active != _appliedScene)
            OnSceneLoaded(active, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
            return;

        _appliedScene = scene;

        // A level lowered by the watchdog takes effect here, between levels, never in the middle of a fight.
        GraphicsTier tier = GraphicsQuality.ResolveTier();
        if (tier != GraphicsQuality.Tier)
            GraphicsQuality.Apply(tier);

        GraphicsSceneLook.Apply(scene);

        // The editor's frame rate says nothing about a phone.
        _sampling = !Application.isEditor && scene.name.StartsWith("Level")
            && GraphicsQuality.Preference == GraphicsPreference.Auto && tier != GraphicsTier.Low;
        _warmUp = WarmUp;
        _sampledTime = 0f;
        _sampledFrames = 0;
    }

    private void Update()
    {
        if (!_sampling)
            return;

        float delta = Time.unscaledDeltaTime;

        if (_warmUp > 0f)
        {
            _warmUp -= delta;
            return;
        }

        // Slow motion and pauses don't say anything about the device.
        if (Time.timeScale < 0.9f || delta > HitchTime)
            return;

        _sampledTime += delta;
        _sampledFrames++;

        if (_sampledTime < SampleDuration)
            return;

        _sampling = false;
        float fps = _sampledFrames / _sampledTime;

        if (fps < MinFps && GraphicsQuality.LowerAutoTier())
            Debug.Log($"[Graphics] {fps:0} fps on {GraphicsQuality.Tier}, next level loads on {GraphicsQuality.AutoTier}");
    }
}
