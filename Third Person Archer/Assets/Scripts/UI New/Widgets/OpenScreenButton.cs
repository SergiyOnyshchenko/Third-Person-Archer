#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UI.Core;

namespace UI.Widgets
{
    [RequireComponent(typeof(Button))]
    public sealed class OpenScreenButton : MonoBehaviour
    {
        [SerializeField] private string _targetScreenId = string.Empty;
        [SerializeField] private bool _reuseCached = true;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (string.IsNullOrEmpty(_targetScreenId)) return;
            if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
                nav.Open(_targetScreenId, args: null, reuseCached: _reuseCached);
        }
    }
}