using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicSet", menuName = "Audio/MusicSet", order = 1)]
public class MusicSet : ScriptableObject
{
    // ── Constants ─────────────────────────────────────────────────────────────
    /// <summary>Path used by BackgroundMusicPlayer to load this asset.</summary>
    public const string ResourcesPath = "Audio/MusicSet";

    // ── Nested type ───────────────────────────────────────────────────────────
    [System.Serializable]
    public class MusicEntry
    {
        [Tooltip("Exact scene name (Build Settings name, without path or extension).")]
        public string SceneName;

        [Header("Level Music")]
        [Tooltip("File name of the level music clip (no extension, no path).\nExample: Level_Forest")]
        public string LevelMusicName;

        [Range(0f, 1f)]
        public float LevelMusicVolume = 0.7f;

        [Header("Victory Music")]
        [Tooltip("File name of the victory music clip (no extension, no path).\nExample: Victory_Fanfare")]
        public string VictoryMusicName;

        [Range(0f, 1f)]
        public float VictoryMusicVolume = 0.8f;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Tooltip("Fallback used when no entry matches the current scene.")]
    [SerializeField] private MusicEntry _defaultEntry = new MusicEntry();

    [Tooltip("One entry per level scene. Matched by scene name at runtime.")]
    [SerializeField] private List<MusicEntry> _entries = new List<MusicEntry>();

    // ── Public API ────────────────────────────────────────────────────────────

    public MusicEntry GetEntry(string sceneName)
    {
        foreach (var entry in _entries)
        {
            if (string.Equals(entry.SceneName, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return _defaultEntry;
    }
}