using System;
using UnityEngine;

namespace Game.Weapons
{
    public static class ReloadUtils
    {
        /// <summary>
        /// Convert a reload time in seconds into a speed multiplier where 1 = baseline.
        /// If you have a better formula (per-weapon baselines), inject it instead.
        /// </summary>
        public static float TimeToSpeedMultiplier(float reloadSeconds, float baselineSeconds = 2.0f)
        {
            reloadSeconds = Mathf.Max(0.05f, reloadSeconds);
            baselineSeconds = Mathf.Max(0.05f, baselineSeconds);
            return baselineSeconds / reloadSeconds;
        }

        /// <summary>Invoke SetReloadMult/SetReloadSpeedMult methods if present (names differ in your controllers).</summary>
        public static void TrySetReloadMultiplier(object controller, float speedMult)
        {
            if (controller == null) return;
            var type = controller.GetType();
            var m1 = type.GetMethod("SetReloadMult");
            if (m1 != null) { m1.Invoke(controller, new object[] { speedMult }); return; }

            var m2 = type.GetMethod("SetReloadSpeedMult");
            if (m2 != null) { m2.Invoke(controller, new object[] { speedMult }); return; }
        }
    }
}
