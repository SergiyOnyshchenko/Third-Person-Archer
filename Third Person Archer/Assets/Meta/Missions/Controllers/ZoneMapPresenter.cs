using UnityEngine;

public sealed class ZoneMapPresenter : MonoBehaviour
{
    private bool _initialized;

    private void OnEnable()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime == null || runtime.Services == null)
            return;

        Init(runtime.Services);
    }

    private void Init(MainMenuServices services)
    {
        if (_initialized) return;
        _initialized = true;

        var buttons = GetComponentsInChildren<MissionSelectorButtonPresenter>(includeInactive: true);
        foreach (var b in buttons)
            b.Init(services);

        // Force a refresh when map appears
        services.NotifyMenuStateChanged();
    }
}