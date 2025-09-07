#nullable enable
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Core
{
    /// <summary>Pure view (no logic). Controllers manipulate this.</summary>
    [DisallowMultipleComponent]
    public class ScreenView : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string _screenId = "UNSET";
        [SerializeField] private UILayer _layer = UILayer.FullScreen;

        [Header("Transition")]
        [SerializeField] private UITransition _enter = UITransition.Fade;
        [SerializeField] private UITransition _exit  = UITransition.Fade;
        [SerializeField] private float _enterDuration = 0.25f;
        [SerializeField] private float _exitDuration  = 0.20f;

        [Header("Refs")]
        [SerializeField] private CanvasGroup? _canvasGroup;
        [SerializeField] private RectTransform? _root;

        [Header("Events")]
        public UnityEvent OnOpened = new();
        public UnityEvent OnClosed = new();
        public UnityEvent OnFocusGained = new();
        public UnityEvent OnFocusLost = new();

        public string ScreenId => _screenId;
        public UILayer Layer => _layer;
        public UITransition EnterTransition => _enter;
        public UITransition ExitTransition => _exit;
        public float EnterDuration => _enterDuration;
        public float ExitDuration => _exitDuration;
        public CanvasGroup CanvasGroup => _canvasGroup ??= gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        public RectTransform Root => _root ??= (RectTransform)transform;

        private void Reset()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (!_root) _root = (RectTransform)transform;
        }
    }
}