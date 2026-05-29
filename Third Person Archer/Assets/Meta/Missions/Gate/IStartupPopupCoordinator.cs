/// <summary>
/// Central coordinator for startup/progression popups.
///
/// Presenters call Submit() with a StartupPopupRequest instead of calling
/// nav.ShowPopup() directly. The coordinator shows requests one at a time,
/// in priority order, and only while the map screen is the active full screen.
///
/// Registered in ServiceLocator as IStartupPopupCoordinator during Awake.
/// </summary>
public interface IStartupPopupCoordinator
{
    /// <summary>
    /// Submit a popup request to the queue.
    /// The coordinator will show it when:
    ///   - no other coordinator-managed popup is currently open, AND
    ///   - the map screen is the active full screen.
    /// If the coordinator is unavailable (not in scene), the request is silently dropped.
    /// </summary>
    void Submit(StartupPopupRequest request);
}
