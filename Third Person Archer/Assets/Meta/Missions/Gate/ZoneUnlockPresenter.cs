using System.Collections.Generic;
using UI.Core;
using UnityEngine;

/// <summary>
/// Shows a one-time popup each time a new zone becomes available.
/// Zone 0 (the starting zone) is always skipped.
/// Zone i (i > 0) is considered unlocked when zone[i-1] has a boss and that boss is defeated.
///
/// Submits requests through StartupPopupCoordinator so popups are shown one at a time
/// and only while the map screen is active.
///
/// Setup: Add this MonoBehaviour to the main menu scene.
/// Populate _zoneIcons with Zone asset → Sprite pairs (one entry per non-starting zone).
/// Register a prefab with MissionTypeUnlockPopupController in ScreenRegistry under "zone_unlock_popup".
/// </summary>
public sealed class ZoneUnlockPresenter : MonoBehaviour
{
    [System.Serializable]
    private struct ZoneIconEntry
    {
        public ZoneData Zone;
        public Sprite   Icon;
    }

    [SerializeField] private string _popupId = "zone_unlock_popup";
    [SerializeField] private List<ZoneIconEntry> _zoneIcons = new();

    private void OnEnable()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            CheckAndSubmit(runtime.Services);
            return;
        }

        MainMenuRuntime.Ready -= OnRuntimeReady;
        MainMenuRuntime.Ready += OnRuntimeReady;
    }

    private void OnDisable()
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
    }

    private void OnRuntimeReady(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
        CheckAndSubmit(services);
    }

    private void CheckAndSubmit(MainMenuServices services)
    {
        var zones = services.ProgressData.AllZones;
        if (zones == null) return;

        for (int i = 1; i < zones.Count; i++) // Start at 1 — skip the starting zone.
        {
            var zone     = zones[i];
            var prevZone = zones[i - 1];
            if (zone == null || prevZone == null) continue;

            // Zone i unlocks when the previous zone's boss has been beaten.
            // If the previous zone has no boss it cannot open the next zone this way.
            if (!prevZone.HasBoss())     continue;
            if (!prevZone.IsBossCompleted()) continue;

            string saveKey = $"zone_unlock_popup_shown_{zone.ID}";
            if (SaveSystem.Load(saveKey, false)) continue;

            string zoneName = !string.IsNullOrEmpty(zone.ZoneName) ? zone.ZoneName : $"Zone {i + 1}";
            var args = new MissionTypeUnlockPopupArgs(
                title: $"{zoneName} Unlocked!",
                body:  "New missions are now available.",
                icon:  GetIcon(zone)
            );

            string capturedKey = saveKey;

            if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
            {
                coord.Submit(new StartupPopupRequest
                {
                    Priority     = StartupPopupPriority.ZoneUnlock,
                    LogicalId    = capturedKey,
                    PopupId      = _popupId,
                    Args         = args,
                    OnBeforeShow = () => SaveSystem.Save(capturedKey, true)
                });
            }
            else
            {
                SaveSystem.Save(capturedKey, true);
                if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                    nav.ShowPopup(_popupId, args);
            }

            break; // At most one zone-unlock popup per session.
        }
    }

    private Sprite GetIcon(ZoneData zone)
    {
        foreach (var entry in _zoneIcons)
            if (entry.Zone == zone) return entry.Icon;
        return null;
    }
}
