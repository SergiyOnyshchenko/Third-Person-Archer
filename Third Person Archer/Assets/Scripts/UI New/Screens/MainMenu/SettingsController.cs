#nullable enable
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class SettingsController : MonoBehaviour, ICanNavigateAway
    {
        [SerializeField] private Button _back = null!;

        private bool _dirty;

        private void Awake()
        {
            _back.onClick.AddListener(() => ServiceLocator.Resolve<IUINavigator>().GoBack());
        }

        public bool CanNavigateAway(out string? reason)
        {
            if (_dirty)
            {
                // In a real app you might show a confirm modal here.
                reason = "You have unsaved changes.";
                // Return false to block leaving; for demo we allow it:
                // return false;
                return true;
            }
            reason = null;
            return true;
        }
    }
}