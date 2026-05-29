using System;

/// <summary>
/// Describes a single startup/progression popup to be shown by StartupPopupCoordinator.
/// </summary>
public sealed class StartupPopupRequest
{
    /// <summary>
    /// Show priority. Lower number = higher priority (shown first).
    /// Use constants from StartupPopupPriority.
    /// </summary>
    public int Priority;

    /// <summary>
    /// Optional deduplication key. If a request with the same LogicalId is already
    /// pending in the coordinator queue, the new request is ignored.
    /// Recommended: use the same string as the one-time save key for the popup.
    /// </summary>
    public string LogicalId;

    /// <summary>
    /// ScreenRegistry popup ID passed to IUINavigator.ShowPopup().
    /// Ignored when CustomShowAction is set.
    /// </summary>
    public string PopupId;

    /// <summary>
    /// Args object passed to IReceivesArgs on the popup prefab.
    /// Null is valid. Ignored when CustomShowAction is set.
    /// </summary>
    public object Args;

    /// <summary>
    /// Called by the coordinator immediately before the popup is shown.
    /// Use this to save the one-time "shown" flag so it is recorded only when
    /// the popup is actually displayed, not when it is queued.
    /// If the coordinator never shows this request the callback is never called.
    /// </summary>
    public Action OnBeforeShow;

    /// <summary>
    /// Optional override for the show step.
    /// When set, the coordinator calls this Action instead of nav.ShowPopup(PopupId, Args).
    /// The coordinator still listens to IUINavigator.PopupShown to capture the popup
    /// instance and track when it closes.
    /// Use for presenters that manage nav.ShowPopup internally (e.g. WeaponUnlockPopupsController).
    /// </summary>
    public Action CustomShowAction;
}
