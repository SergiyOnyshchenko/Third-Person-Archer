using System.Collections.Generic;
using UI.Core;
using UnityEngine;

/// <summary>
/// Shows a one-time intro popup the first time the player selects the Boss tab
/// after a Boss mission has become available in a zone.
///
/// Fires on tab selection (not startup) — intentionally avoids stacking with
/// ZoneCompletePresenter / ZoneUnlockPresenter which fire on map open.
///
/// Per-zone title, body, and boss image are configured in Inspector via _bossEntries.
/// Uses MissionTypeUnlockPopupController (boss_intro_popup registry ID).
///
/// Setup: Add this MonoBehaviour to the main menu scene.
/// Populate _bossEntries with one entry per zone that has a boss.
/// </summary>
public sealed class BossIntroPresenter : MonoBehaviour
{
    [System.Serializable]
    private struct BossIntroEntry
    {
        public ZoneData Zone;
        public string   Title;
        [TextArea(2, 5)]
        public string   Body;
        public Sprite   Image;
    }

    [SerializeField] private string _popupId = "boss_intro_popup";
    [SerializeField] private List<BossIntroEntry> _bossEntries = new();

    private MainMenuServices _services;
    private bool _initialized;

    private void OnEnable()
    {
        TryInitOrSubscribe();
    }

    private void OnDisable()
    {
        if (!_initialized)
            MainMenuRuntime.Ready -= OnRuntimeReady;
    }

    private void OnDestroy()
    {
        if (_services != null)
            _services.ProgressData.OnMissionTypeChanged.RemoveListener(OnMissionTypeChanged);
    }

    private void TryInitOrSubscribe()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            Init(runtime.Services);
            return;
        }

        MainMenuRuntime.Ready -= OnRuntimeReady;
        MainMenuRuntime.Ready += OnRuntimeReady;
    }

    private void OnRuntimeReady(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
        Init(services);
    }

    private void Init(MainMenuServices services)
    {
        if (_initialized) return;
        _initialized = true;
        _services = services;
        _services.ProgressData.OnMissionTypeChanged.AddListener(OnMissionTypeChanged);
    }

    private void OnMissionTypeChanged()
    {
        if (_services == null) return;
        if (_services.ProgressData.MissionType != MissionType.Boss) return;

        var zone = _services.ProgressData.Zone;
        if (zone == null || !zone.IsBossUnlocked()) return;

        string saveKey = $"boss_intro_popup_shown_{zone.ID}";
        if (SaveSystem.Load(saveKey, false)) return;

        // Flag saved before show — popup fires on explicit user action (tab select),
        // so we record it immediately to avoid re-triggering on fast re-navigation.
        SaveSystem.Save(saveKey, true);

        BossIntroEntry entry = FindEntry(zone);

        string title = !string.IsNullOrEmpty(entry.Title)
            ? entry.Title
            : "Boss Challenge!";

        string body = !string.IsNullOrEmpty(entry.Body)
            ? entry.Body
            : "Defeat the Boss to unlock the next zone.";

        var args = new MissionTypeUnlockPopupArgs(title, body, entry.Image);

        if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
            nav.ShowPopup(_popupId, args);
    }

    private BossIntroEntry FindEntry(ZoneData zone)
    {
        foreach (var entry in _bossEntries)
            if (entry.Zone == zone) return entry;
        return default;
    }
}
