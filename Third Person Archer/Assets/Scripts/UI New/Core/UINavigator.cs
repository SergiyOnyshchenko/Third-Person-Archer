#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Core
{
    /// <summary>Main router + stack-based navigation with layers.</summary>
    public interface IUINavigator
    {
        Transform FullScreensRoot { get; }
        Transform ModalsRoot { get; }
        Transform OverlaysRoot { get; }
        Transform PopupsRoot { get; }

        /// <summary>Open a full-screen, push onto stack.</summary>
        void Open(string screenId, object? args = null, bool reuseCached = true);
        /// <summary>Go back in full-screen stack (if allowed).</summary>
        void GoBack();
        /// <summary>Show a modal (stacked).</summary>
        void ShowModal(string screenId, object? args = null);
        /// <summary>Close top modal if any.</summary>
        void CloseTopModal();
        /// <summary>Show/hide overlay (id must map to an overlay prefab).</summary>
        void ShowOverlay(string screenId);
        void HideOverlay(string screenId);
        /// <summary>Show a popup (toast) with optional args.</summary>
        void ShowPopup(string popupId, object? args = null);
    }

    public sealed class UINavigator : MonoBehaviour, IUINavigator
    {
        [Header("Layer Roots")]
        [SerializeField] private Transform _fullScreensRoot = null!;
        [SerializeField] private Transform _overlaysRoot = null!;
        [SerializeField] private Transform _modalsRoot = null!;
        [SerializeField] private Transform _popupsRoot = null!;

        [Header("Registries (checked in order)")]
        [SerializeField] private ScreenRegistry[] _registries = Array.Empty<ScreenRegistry>();

        private readonly List<ScreenView> _screenStack = new();
        private readonly List<ScreenView> _modalStack = new();
        private readonly Dictionary<string, ScreenView> _cache = new();
        private ITransitionPlayer _transition = null!;
        private IUIAnalytics _analytics = null!;
        private IScreenRegistry _composedRegistry = null!;

        private bool _isTransitionRunning;
        private readonly Queue<IEnumerator> _queue = new();

        public Transform FullScreensRoot => _fullScreensRoot;
        public Transform ModalsRoot => _modalsRoot;
        public Transform OverlaysRoot => _overlaysRoot;
        public Transform PopupsRoot => _popupsRoot;

        private void Awake()
        {
            _transition = ServiceLocator.TryResolve<ITransitionPlayer>(out var t) ? t : new DOTweenFadeTransitionPlayer();
            _analytics  = ServiceLocator.TryResolve<IUIAnalytics>(out var a) ? a : new NullUIAnalytics();

            _composedRegistry = new ChainedRegistry(_registries);
            ServiceLocator.Register<IUINavigator>(this);
            ServiceLocator.Register<IScreenRegistry>(_composedRegistry);
        }

        #region API
        public void Open(string screenId, object? args = null, bool reuseCached = true)
        {
            Enqueue(OpenRoutine(screenId, args, reuseCached));
        }

        public void GoBack()
        {
            if (_screenStack.Count <= 1) return; // nothing to pop past root
            var top = _screenStack[^1];

            if (!CanLeave(top)) return;
            Enqueue(BackRoutine());
        }

        public void ShowModal(string screenId, object? args = null)
        {
            Enqueue(ShowModalRoutine(screenId, args));
        }

        public void CloseTopModal()
        {
            if (_modalStack.Count == 0) return;
            Enqueue(CloseTopModalRoutine());
        }

        public void ShowOverlay(string screenId) => Enqueue(ShowOverlayRoutine(screenId));
        public void HideOverlay(string screenId) => Enqueue(HideOverlayRoutine(screenId));
        public void ShowPopup(string popupId, object? args = null) => Enqueue(ShowPopupRoutine(popupId, args));
        #endregion

        #region Routines
        private IEnumerator OpenRoutine(string screenId, object? args, bool reuseCached)
        {
            // Instantiate or reuse target view
            var next = reuseCached && _cache.TryGetValue(screenId, out var cached) ? cached : InstantiateView(screenId, _fullScreensRoot);
            if (!_cache.ContainsKey(screenId)) _cache[screenId] = next;

            // Hide current top (lose focus)
            if (_screenStack.Count > 0)
            {
                var current = _screenStack[^1];
                current.OnFocusLost.Invoke();
                yield return _transition.PlayExit(current); // fade out
            }

            // Apply args if controller supports it
            ApplyArgsIfAny(next, args);

            // Show new
            yield return _transition.PlayEnter(next);
            next.OnOpened.Invoke();
            next.OnFocusGained.Invoke();
            _analytics.OnScreenOpened(next.ScreenId);

            if (_screenStack.Count == 0 || _screenStack[^1] != next)
                _screenStack.Add(next);
        }

        private IEnumerator BackRoutine()
        {
            var top = _screenStack[^1];
            top.OnFocusLost.Invoke();
            yield return _transition.PlayExit(top);
            top.OnClosed.Invoke();
            _analytics.OnScreenClosed(top.ScreenId);

            _screenStack.RemoveAt(_screenStack.Count - 1);

            var newTop = _screenStack[^1];
            yield return _transition.PlayEnter(newTop);
            newTop.OnFocusGained.Invoke();
        }

        private IEnumerator ShowModalRoutine(string screenId, object? args)
        {
            var modal = InstantiateView(screenId, _modalsRoot);
            ApplyArgsIfAny(modal, args);
            yield return _transition.PlayEnter(modal);
            modal.OnOpened.Invoke();
            modal.OnFocusGained.Invoke();
            _modalStack.Add(modal);
            _analytics.OnModalOpened(modal.ScreenId);
        }

        private IEnumerator CloseTopModalRoutine()
        {
            var m = _modalStack[^1];
            m.OnFocusLost.Invoke();
            yield return _transition.PlayExit(m);
            m.OnClosed.Invoke();
            _modalStack.RemoveAt(_modalStack.Count - 1);
            Destroy(m.gameObject);
            _analytics.OnModalClosed(m.ScreenId);
        }

        private IEnumerator ShowOverlayRoutine(string screenId)
        {
            var ov = InstantiateView(screenId, _overlaysRoot);
            _transition.InstantShow(ov);
            ov.OnOpened.Invoke();
            yield break;
        }

        private IEnumerator HideOverlayRoutine(string screenId)
        {
            // Find by id under overlays
            for (int i = OverlaysRoot.childCount - 1; i >= 0; i--)
            {
                var v = OverlaysRoot.GetChild(i).GetComponent<ScreenView>();
                if (v != null && v.ScreenId == screenId)
                {
                    _transition.InstantHide(v);
                    Destroy(v.gameObject);
                    break;
                }
            }
            yield break;
        }

        private IEnumerator ShowPopupRoutine(string popupId, object? args)
        {
            var p = InstantiateView(popupId, _popupsRoot);
            ApplyArgsIfAny(p, args);
            _transition.InstantShow(p);
            p.OnOpened.Invoke();
            _analytics.OnPopupShown(popupId);
            // Popups/autodestroy handled by the popup controller itself (e.g., timer)
            yield break;
        }
        #endregion

        private ScreenView InstantiateView(string id, Transform parent)
        {
            if (!_composedRegistry.TryGetPrefab(id, out var prefab))
                throw new ArgumentException($"Screen id '{id}' not found in any registry (searched {_registries.Length}).");

            var instance = Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            instance.CanvasGroup.alpha = 0f;
            instance.CanvasGroup.interactable = false;
            instance.CanvasGroup.blocksRaycasts = false;
            return instance;
        }

        private sealed class ChainedRegistry : IScreenRegistry
        {
            private readonly ScreenRegistry[] _registries;
            public ChainedRegistry(ScreenRegistry[] registries) => _registries = registries ?? Array.Empty<ScreenRegistry>();

            public bool TryGetPrefab(string screenId, out ScreenView prefab)
            {
                for (int i = 0; i < _registries.Length; i++)
                {
                    var r = _registries[i];
                    if (r != null && r.TryGetPrefab(screenId, out prefab))
                        return true;
                }
                prefab = null!;
                return false;
            }
        }

        private static bool CanLeave(ScreenView view)
        {
            // Allow any controller on this GO to veto
            var controllers = view.GetComponents<MonoBehaviour>();
            foreach (var c in controllers)
            {
                if (c is ICanNavigateAway guard && !guard.CanNavigateAway(out _))
                    return false;
            }
            return true;
        }

        private static void ApplyArgsIfAny(ScreenView view, object? args)
        {
            if (args == null) return;
            foreach (var c in view.GetComponents<MonoBehaviour>())
            {
                var ifaces = c.GetType().GetInterfaces();
                foreach (var i in ifaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReceivesArgs<>))
                    {
                        var t = i.GetGenericArguments()[0];
                        if (args.GetType() == t)
                        {
                            // Validate + Apply
                            var validate = i.GetMethod("ValidateArgs");
                            var apply = i.GetMethod("ApplyArgs");
                            if (validate != null && apply != null)
                            {
                                var ok = (bool)validate.Invoke(c, new object[] { args })!;
                                if (ok) apply.Invoke(c, new object[] { args });
                            }
                        }
                    }
                }
            }
        }

        #region Queue (no overlapping transitions)
        private void Enqueue(IEnumerator routine)
        {
            _queue.Enqueue(routine);
            if (!_isTransitionRunning) StartCoroutine(RunQueue());
        }

        private IEnumerator RunQueue()
        {
            _isTransitionRunning = true;
            while (_queue.Count > 0)
            {
                yield return StartCoroutine(_queue.Dequeue());
            }
            _isTransitionRunning = false;
        }
        #endregion
    }
}