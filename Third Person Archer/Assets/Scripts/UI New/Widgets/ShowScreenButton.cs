#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Widgets
{
    public enum ShowScreenType
    {
        Modal,
        Overlay,
        Popup
    }

    [RequireComponent(typeof(Button))]
    public sealed class ShowScreenButton : MonoBehaviour
    {
        [SerializeField] private string _targetId = string.Empty;
        [SerializeField] private ShowScreenType _type = ShowScreenType.Modal;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (string.IsNullOrEmpty(_targetId)) return;
            if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

            switch (_type)
            {
                case ShowScreenType.Modal:
                    nav.ShowModal(_targetId);
                    break;
                case ShowScreenType.Overlay:
                    nav.ShowOverlay(_targetId);
                    break;
                case ShowScreenType.Popup:
                    nav.ShowPopup(_targetId);
                    break;
            }
        }
    }
}

