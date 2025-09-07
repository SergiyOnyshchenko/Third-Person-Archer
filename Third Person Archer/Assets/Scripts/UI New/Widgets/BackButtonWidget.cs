#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Widgets
{
    [RequireComponent(typeof(Button))]
    public sealed class BackButtonWidget : MonoBehaviour
    {
        [SerializeField] private ScreenView? _contextView; // optional explicit reference

        private void Awake()
        {
            // wire button
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(OnBackPressed);

            // lazy-resolve view if not assigned
            if (_contextView == null)
                _contextView = FindNearestScreenView(transform);
        }

        public void OnBackPressed()
        {
            if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

            var view = _contextView ?? FindNearestScreenView(transform);
            if (view == null) { nav.GoBack(); return; }

            switch (view.Layer)
            {
                case UILayer.Modal:
                    nav.CloseTopModal();
                    break;

                case UILayer.Overlay:
                    nav.HideOverlay(view.ScreenId);
                    break;

                case UILayer.Popup:
                    // popups are transient; close this instance
                    Destroy(view.gameObject);
                    break;

                default: // FullScreen (or anything else)
                    nav.GoBack();
                    break;
            }
        }

        private static ScreenView? FindNearestScreenView(Transform start)
        {
            Transform? t = start;
            while (t != null)
            {
                if (t.TryGetComponent<ScreenView>(out var sv))
                    return sv;
                t = t.parent;
            }
            return null;
        }
    }
}