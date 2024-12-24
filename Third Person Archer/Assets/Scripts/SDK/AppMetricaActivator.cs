
using UnityEngine;

public static class AppMetricaActivator
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
        /*
        AppMetrica.Activate(new AppMetricaConfig("a6abc11e-a62a-487b-a969-ac15c5aca7da")
        {
            FirstActivationAsUpdate = !IsFirstLaunch(),
        });
        */
    }

    private static bool IsFirstLaunch()
    {
        // Implement logic to detect whether the app is opening for the first time.
        // For example, you can check for files (settings, databases, and so on),
        // which the app creates on its first launch.
        return true;
    }
}
