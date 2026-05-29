using UI.Core;
using UnityEngine;

/// <summary>
/// Shows a one-time intro popup the first time the player selects the Boss tab
/// after a Boss mission has become available in a zone.
///
/// Fires on tab selection (not startup) to avoid same-session collision with
/// ZoneCompletePresenter (Phase B) which fires automatically when Campaign is done.
///
/// Reuses CelebrationPopupController via the "CelebrationPopup" registry ID.
///
/// Setup: Add this MonoBehaviour to the main menu scene.
/// Set _popupId = "CelebrationPopup".
/// </summary>
public sealed class BossIntroPresenter : MonoBehaviour
{
    [SerializeField] private string _popupId = "CelebrationPopup";

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
        if (_services == null)
            return;

        if (_services.ProgressData.MissionType != MissionType.Boss)
            return;

        var zone = _services.ProgressData.Zone;
        if (zone == null || !zone.IsBossUnlocked())
            return;

        string saveKey = $"boss_intro_popup_shown_{zone.ID}";
        if (SaveSystem.Load(saveKey, false))
            return;

        SaveSystem.Save(saveKey, true);

        string zoneName = !string.IsNullOrEmpty(zone.ZoneName) ? zone.ZoneName : "this zone";

        string body =
            $"The Boss is the final challenge of {zoneName}.\n\n" +
            "Defeating the Boss opens the next zone and gives a large reward.\n\n" +
            "Boss missions require Crossbow Power — not just any weapon. " +
            "Play Sniper missions to earn Crossbow Tokens and prepare your Crossbow before you challenge the Boss.\n\n" +
            "When ready, select the Boss tab and hit Play.";

        var args = new CelebrationPopupArgs(
            title: "Boss Mission!",
            body: body
        );

        if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
            nav.ShowPopup(_popupId, args);
    }
}
