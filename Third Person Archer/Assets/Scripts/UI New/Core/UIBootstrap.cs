#nullable enable
using UnityEngine;

namespace UI.Core
{
    /// <summary>Sets up services and opens initial screen.</summary>
    public sealed class UIBootstrap : MonoBehaviour
    {
        [SerializeField] private UINavigator _navigator = null!;
        [SerializeField] private ScreenRegistry _registry = null!;
        [SerializeField] private string _initialScreenId = "MainMenu";

        private void Awake()
        {
            // Register services not owned by navigator
            if (!ServiceLocator.TryResolve<ITransitionPlayer>(out _)) ServiceLocator.Register<ITransitionPlayer>(new DOTweenFadeTransitionPlayer());
            if (!ServiceLocator.TryResolve<IUIAnalytics>(out _)) ServiceLocator.Register<IUIAnalytics>(new NullUIAnalytics());

            // Ensure navigator has registry reference (if not given in inspector)
            if (_navigator && _registry) { /* already wired by inspector */ }
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(_initialScreenId))
                ServiceLocator.Resolve<IUINavigator>().Open(_initialScreenId);
        }
    }
}
