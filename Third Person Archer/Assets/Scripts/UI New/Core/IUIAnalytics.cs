#nullable enable
namespace UI.Core
{
    public interface IUIAnalytics
    {
        void OnScreenOpened(string screenId);
        void OnScreenClosed(string screenId);
        void OnModalOpened(string screenId);
        void OnModalClosed(string screenId);
        void OnPopupShown(string popupId);
    }

    /// <summary>Null-object default to avoid null checks.</summary>
    public sealed class NullUIAnalytics : IUIAnalytics
    {
        public void OnScreenOpened(string screenId) {}
        public void OnScreenClosed(string screenId) {}
        public void OnModalOpened(string screenId) {}
        public void OnModalClosed(string screenId) {}
        public void OnPopupShown(string popupId) {}
    }
}
