using System;

namespace UI.HUD
{
    /// <summary>Global events to announce reload start/end with a known duration (seconds).</summary>
    public static class ReloadSignals
    {
        public static event Action<float> OnReloadStarted; // duration
        public static event Action OnReloadEnded;

        public static void Start(float durationSeconds)
            => OnReloadStarted?.Invoke(Math.Max(0f, durationSeconds));

        public static void End()
            => OnReloadEnded?.Invoke();
    }
}