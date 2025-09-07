#nullable enable
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _playButton = null!;
        [SerializeField] private Button _settingsButton = null!;
        [SerializeField] private Button _quitButton = null!;

        private ScreenView _view = null!;
        private IUINavigator _nav = null!;

        private void Awake()
        {
            _view = GetComponent<ScreenView>();
            _nav = ServiceLocator.Resolve<IUINavigator>();

            _playButton.onClick.AddListener(OnPlay);
            _settingsButton.onClick.AddListener(() => _nav.Open("Settings"));
#if UNITY_ANDROID || UNITY_IOS
            _quitButton.gameObject.SetActive(false);
#else
            _quitButton.onClick.AddListener(Application.Quit);
#endif
        }

        private void OnPlay()
        {
            // Example deep link to MissionMap with typed args
            var args = new MissionMapArgs { ZoneId = 1, HighlightAvailable = true };
            _nav.Open("MissionMap", args);
        }
    }
}