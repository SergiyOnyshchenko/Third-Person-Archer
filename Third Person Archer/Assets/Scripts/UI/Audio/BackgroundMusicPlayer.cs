using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static BackgroundMusicPlayer Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Behaviour")]
    [Tooltip("When TRUE, level music will NOT start on OnGameStarted.\n" +
             "Call PlayLevelMusic() manually from another script instead.")]
    [SerializeField] private bool _manualLevelMusicStart;

    [Tooltip("Crossfade duration in seconds when switching tracks (0 = instant).")]
    [Min(0f)]
    [SerializeField] private float _crossfadeDuration = 1.2f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private MusicSet              _musicSet;
    private MusicSet.MusicEntry   _activeEntry;

    // Two AudioSources allow clean crossfades.
    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private AudioSource _activeSource;
    private AudioSource _inactiveSource;

    private Coroutine _fadeRoutine;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BackgroundMusicPlayer] Duplicate detected – destroying.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildAudioSources();
        LoadMusicSet();
    }

    private void OnEnable()
    {
        RuntimeMissionEventManager.OnGameStarted.AddListener(HandleGameStarted);
        RuntimeMissionEventManager.OnGameContinued.AddListener(HandleGameContinued);
        RuntimeMissionEventManager.OnGameFinished.AddListener(HandleGameFinished);
    }

    private void OnDisable()
    {
        RuntimeMissionEventManager.OnGameStarted.RemoveListener(HandleGameStarted);
        RuntimeMissionEventManager.OnGameContinued.RemoveListener(HandleGameContinued);
        RuntimeMissionEventManager.OnGameFinished.RemoveListener(HandleGameFinished);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the level music track.
    /// Called automatically on OnGameStarted unless _manualLevelMusicStart is true.
    /// </summary>
    public void PlayLevelMusic()
    {
        if (_activeEntry == null) return;

        var clip = LoadClip(_activeEntry.LevelMusicName);
        if (clip == null) return;

        PlayClip(clip, _activeEntry.LevelMusicVolume, loop: true);
    }

    /// <summary>
    /// Starts the victory music track.
    /// Called automatically on OnGameFinished.
    /// </summary>
    public void PlayVictoryMusic()
    {
        if (_activeEntry == null) return;

        var clip = LoadClip(_activeEntry.VictoryMusicName);
        if (clip == null) return;

        PlayClip(clip, _activeEntry.VictoryMusicVolume, loop: false);
    }

    /// <summary>Stops all music with a fade-out.</summary>
    public void StopMusic() => CrossfadeTo(null, 0f, false);

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleGameStarted()
    {
        if (!_manualLevelMusicStart)
            PlayLevelMusic();
    }

    private void HandleGameContinued()
    {
        // Unpause if audio was paused (e.g. during time-scale 0 pause).
        if (_activeSource != null && !_activeSource.isPlaying && _activeSource.clip != null)
            _activeSource.UnPause();
    }

    private void HandleGameFinished()
    {
        PlayVictoryMusic();
    }

    // ── Init helpers ──────────────────────────────────────────────────────────

    private void BuildAudioSources()
    {
        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();

        InitSource(_sourceA);
        InitSource(_sourceB);

        _activeSource   = _sourceA;
        _inactiveSource = _sourceB;
    }

    private static void InitSource(AudioSource src)
    {
        src.playOnAwake  = false;
        src.spatialBlend = 0f; // 2-D
        src.volume       = 0f;
    }

    private void LoadMusicSet()
    {
        _musicSet = Resources.Load<MusicSet>(MusicSet.ResourcesPath);

        if (_musicSet == null)
        {
            Debug.LogError($"[BackgroundMusicPlayer] MusicSet not found at Resources/{MusicSet.ResourcesPath}. " +
                           "Create it via Assets → Create → Audio → MusicSet and place it there.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        _activeEntry = _musicSet.GetEntry(sceneName);

        Debug.Log($"[BackgroundMusicPlayer] Scene '{sceneName}' → " +
                  $"level: '{_activeEntry.LevelMusicName}', " +
                  $"victory: '{_activeEntry.VictoryMusicName}'");
    }

    // ── Clip loading ──────────────────────────────────────────────────────────

    private static AudioClip LoadClip(string musicName)
    {
        if (string.IsNullOrWhiteSpace(musicName))
        {
            Debug.LogWarning("[BackgroundMusicPlayer] Music name is empty – skipping.");
            return null;
        }

        string path = $"Audio/BGM/{musicName}";
        var clip = Resources.Load<AudioClip>(path);

        if (clip == null)
            Debug.LogWarning($"[BackgroundMusicPlayer] AudioClip not found at Resources/{path}");

        return clip;
    }

    // ── Playback & crossfade ──────────────────────────────────────────────────

    private void PlayClip(AudioClip clip, float targetVolume, bool loop)
    {
        // Already playing the same clip – do nothing.
        if (_activeSource.clip == clip && _activeSource.isPlaying)
            return;

        CrossfadeTo(clip, targetVolume, loop);
    }

    private void CrossfadeTo(AudioClip clip, float targetVolume, bool loop)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(CrossfadeRoutine(clip, targetVolume, loop));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip, float targetVolume, bool loop)
    {
        // Swap roles: the incoming source becomes "active".
        (_activeSource, _inactiveSource) = (_inactiveSource, _activeSource);

        float fadeOut  = _inactiveSource.volume;
        float duration = _crossfadeDuration;
        float elapsed  = 0f;

        if (clip != null)
        {
            _activeSource.clip   = clip;
            _activeSource.loop   = loop;
            _activeSource.volume = 0f;
            _activeSource.Play();
        }

        if (duration <= 0f)
        {
            _activeSource.volume   = clip != null ? targetVolume : 0f;
            _inactiveSource.volume = 0f;
            _inactiveSource.Stop();
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (clip != null)
                _activeSource.volume = Mathf.Lerp(0f, targetVolume, t);

            _inactiveSource.volume = Mathf.Lerp(fadeOut, 0f, t);

            yield return null;
        }

        _activeSource.volume   = clip != null ? targetVolume : 0f;
        _inactiveSource.volume = 0f;
        _inactiveSource.Stop();
        _inactiveSource.clip = null;
    }
}