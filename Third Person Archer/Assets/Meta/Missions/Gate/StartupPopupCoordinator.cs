#nullable enable
using System.Collections;
using System.Collections.Generic;
using UI.Core;
using UnityEngine;

/// <summary>
/// Coordinates all startup/progression popup requests so that they are shown
/// one at a time and only while the map screen is the active full screen.
///
/// LIFECYCLE
/// - Awake:  registers itself as IStartupPopupCoordinator in ServiceLocator.
///           Runs before default-order MonoBehaviours so presenters can resolve it
///           in their OnEnable.
/// - Start:  resolves IUINavigator, subscribes to ScreenOpened.
///           This is intentionally deferred to Start so the navigator has time
///           to register itself in Awake.
/// - OnDestroy: unregisters from ServiceLocator and cleans up event subscriptions.
///
/// MAP SCREEN DETECTION
/// The coordinator tracks the map screen via two complementary mechanisms:
///   1. IUINavigator.ScreenOpened — fired when the map screen is first opened
///      (navigated to from another full screen).
///   2. ScreenView.OnFocusGained / OnFocusLost — fired when the map screen
///      regains or loses focus during a GoBack() navigation.
/// Both are needed because GoBack() does not fire ScreenOpened for the
/// screen that re-appears beneath the closed one.
///
/// POPUP CLOSE DETECTION
/// After showing a popup the coordinator subscribes to IUINavigator.PopupShown
/// to capture the ScreenView instance.  A WaitForClose coroutine then polls
/// every frame until that instance is destroyed (all popup controllers close
/// themselves via Destroy(gameObject)).  Once destroyed, the next request is shown.
///
/// SHOW TIMING
/// The coordinator calls nav.ShowPopup() from within an event handler that fires
/// while UINavigator's internal RunQueue coroutine may still be processing the
/// map-screen open transition.  In that case nav.ShowPopup() enqueues the popup
/// and it is shown in the next RunQueue iteration (same or next frame).
/// The PopupShown subscription therefore stays open until the event actually fires.
///
/// FALLBACK
/// If no coordinator is registered (missing from scene), all presenter fallback
/// paths call nav.ShowPopup() directly, preserving the old behaviour.
///
/// SETUP
/// 1. Add this component to a GameObject in the main menu scene.
/// 2. Set _mapScreenId to match the ScreenRegistry ID of the zone map screen ("map").
/// 3. Ensure this component is in the scene alongside all startup presenters.
/// </summary>
[DefaultExecutionOrder(-100)]   // register before default-order presenters run their Awake/OnEnable
public sealed class StartupPopupCoordinator : MonoBehaviour, IStartupPopupCoordinator
{
    [SerializeField] private string _mapScreenId = "map";

    private IUINavigator? _nav;

    // Pending requests, kept sorted by ascending priority (index 0 = next to show).
    private readonly List<StartupPopupRequest> _pending = new();

    // Logical IDs of all requests currently in _pending (for deduplication).
    private readonly HashSet<string> _pendingIds = new();

    private bool _popupActive;
    private bool _mapActive;

    // Captured once when the map screen ScreenView is first seen; used to
    // subscribe to its OnFocusGained / OnFocusLost events.
    private ScreenView? _mapScreenView;

    // Flag set when coordinator has subscribed to PopupShown and is waiting
    // for the event to fire.  Prevents double-subscription if TryShowNext is
    // called again before PopupShown fires.
    private bool _awaitingPopupShown;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        ServiceLocator.Register<IStartupPopupCoordinator>(this);
    }

    private void Start()
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out _nav))
        {
            Debug.LogError("StartupPopupCoordinator: IUINavigator not found. " +
                           "Ensure UINavigator.Awake runs before coordinator Start, " +
                           "or check Script Execution Order.");
            enabled = false;
            return;
        }

        _nav.ScreenOpened += OnScreenOpened;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<IStartupPopupCoordinator>();

        if (_nav != null)
        {
            _nav.ScreenOpened -= OnScreenOpened;
            // Safety: if destroyed while waiting for PopupShown, clean up the subscription.
            _nav.PopupShown -= OnPopupShownForCurrentRequest;
        }

        if (_mapScreenView != null)
        {
            _mapScreenView.OnFocusGained.RemoveListener(OnMapFocusGained);
            _mapScreenView.OnFocusLost.RemoveListener(OnMapFocusLost);
        }
    }

    // -------------------------------------------------------------------------
    // IStartupPopupCoordinator
    // -------------------------------------------------------------------------

    public bool IsIdle => !_popupActive && !_awaitingPopupShown && _pending.Count == 0;

    public void Submit(StartupPopupRequest request)
    {
        if (request == null) return;

        // Deduplication: ignore if same logical ID is already queued.
        if (!string.IsNullOrEmpty(request.LogicalId) && _pendingIds.Contains(request.LogicalId))
            return;

        if (!string.IsNullOrEmpty(request.LogicalId))
            _pendingIds.Add(request.LogicalId);

        // Insert in ascending priority order (stable: equal priority → appended at end).
        int i = 0;
        while (i < _pending.Count && _pending[i].Priority <= request.Priority)
            i++;
        _pending.Insert(i, request);

        TryShowNext();
    }

    // -------------------------------------------------------------------------
    // Internal show flow
    // -------------------------------------------------------------------------

    private void TryShowNext()
    {
        if (_popupActive) return;
        if (!_mapActive) return;
        if (_pending.Count == 0) return;
        if (_awaitingPopupShown) return;  // already waiting for nav to show the queued popup

        var request = _pending[0];
        _pending.RemoveAt(0);

        if (!string.IsNullOrEmpty(request.LogicalId))
            _pendingIds.Remove(request.LogicalId);

        ShowRequest(request);
    }

    private void ShowRequest(StartupPopupRequest request)
    {
        _popupActive = true;
        _awaitingPopupShown = true;

        // Save the one-time flag immediately before displaying.
        request.OnBeforeShow?.Invoke();

        // Subscribe to PopupShown BEFORE triggering the show call so we never
        // miss the event even if it fires synchronously.
        _nav!.PopupShown += OnPopupShownForCurrentRequest;

        if (request.CustomShowAction != null)
        {
            request.CustomShowAction.Invoke();
        }
        else if (!string.IsNullOrEmpty(request.PopupId))
        {
            _nav.ShowPopup(request.PopupId, request.Args);
        }
        else
        {
            // Nothing to show — release immediately.
            _nav.PopupShown -= OnPopupShownForCurrentRequest;
            _awaitingPopupShown = false;
            _popupActive = false;
            TryShowNext();
        }
    }

    private void OnPopupShownForCurrentRequest(ScreenView view)
    {
        _nav!.PopupShown -= OnPopupShownForCurrentRequest;
        _awaitingPopupShown = false;
        StartCoroutine(WaitForClose(view));
    }

    private IEnumerator WaitForClose(ScreenView popup)
    {
        // Poll every frame until the popup GameObject is destroyed.
        // All popup controllers in this project close themselves via Destroy(gameObject).
        while (popup != null)
            yield return null;

        _popupActive = false;
        TryShowNext();
    }

    // -------------------------------------------------------------------------
    // Map screen tracking
    // -------------------------------------------------------------------------

    private void OnScreenOpened(ScreenView view)
    {
        if (view.ScreenId == _mapScreenId)
        {
            // First time the map screen is seen: subscribe to its focus events so
            // we can detect map re-activation after GoBack() (which does not fire
            // ScreenOpened for the re-appearing screen).
            if (_mapScreenView == null)
            {
                _mapScreenView = view;
                view.OnFocusGained.AddListener(OnMapFocusGained);
                view.OnFocusLost.AddListener(OnMapFocusLost);
            }
            // ScreenOpened fires → map is active (OnFocusGained will also fire
            // shortly after from OpenRoutine, but calling here is harmless).
            OnMapFocusGained();
        }
        else
        {
            // A different full screen opened — map is no longer in focus.
            _mapActive = false;
        }
    }

    private void OnMapFocusGained()
    {
        _mapActive = true;
        TryShowNext();
    }

    private void OnMapFocusLost()
    {
        _mapActive = false;
    }
}
