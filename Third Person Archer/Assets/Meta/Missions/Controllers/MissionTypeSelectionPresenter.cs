using UnityEngine;

public sealed class MissionTypeSelectionPresenter : MonoBehaviour
{
    private MainMenuServices _services;
    private bool _initialized;

    private void OnEnable()
    {
        TryInitOrSubscribe();
    }

    private void OnDisable()
    {
        if (!_initialized)
            MainMenuRuntime.Ready -= OnRuntimeReady;
    }

    private void TryInitOrSubscribe()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            Init(runtime.Services);
            return;
        }

        // Runtime not ready yet (screen spawned very early) => wait for Ready event
        MainMenuRuntime.Ready -= OnRuntimeReady;
        MainMenuRuntime.Ready += OnRuntimeReady;
    }

    private void OnRuntimeReady(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
        Init(services);
    }

    public void Init(MainMenuServices services)
    {
        if (_initialized) return;
        _initialized = true;
        _services = services;
    }

    public void SelectMissionType(MissionType type)
    {
        if (_services == null) return;

        _services.Progress.SelectMissionType(type);
        _services.NotifyMenuStateChanged();
    }
}