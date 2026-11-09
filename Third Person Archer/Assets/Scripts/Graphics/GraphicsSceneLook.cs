using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Per-scene part of the quality levels, done at load so no scene has to be edited:
/// ambient gradient, which objects cast shadows, and the global look volume (vignette everywhere, grade and bloom on High).
/// Panini stays on every level: it comes from the URP assets' default volume profile.
/// </summary>
public static class GraphicsSceneLook
{
    private const float VolumePriority = 10f;

    private static Volume _volume;

    public static void Apply(Scene scene)
    {
        if (!scene.IsValid())
            return;

        GraphicsLookConfig config = GraphicsLookConfig.Instance;
        GraphicsTier tier = GraphicsQuality.Tier;

        ApplyAmbient(config);

        if (tier != GraphicsTier.Low)
            FilterShadowCasters(config.MinCasterSize);

        ApplyVolume(scene, config, tier);
    }

    private static void ApplyAmbient(GraphicsLookConfig config)
    {
        // Already a gradient (converted earlier, or authored that way): leave it.
        if (RenderSettings.ambientMode != AmbientMode.Flat)
            return;

        Color color = RenderSettings.ambientLight;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = color * config.SkyMultiplier;
        RenderSettings.ambientEquatorColor = color * config.EquatorMultiplier;
        RenderSettings.ambientGroundColor = Color.Lerp(color, config.GroundTint, config.GroundTintAmount) * config.GroundMultiplier;
    }

    /// <summary>Pebbles, planks and small props stop casting: they cost a draw each in the shadow pass and the shadow is invisible anyway.</summary>
    private static void FilterShadowCasters(float minSize)
    {
        foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (renderer.shadowCastingMode == ShadowCastingMode.Off || renderer is SkinnedMeshRenderer)
                continue;

            if (renderer is MeshRenderer && renderer.bounds.size.magnitude >= minSize)
                continue;

            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private static void ApplyVolume(Scene scene, GraphicsLookConfig config, GraphicsTier tier)
    {
        if (_volume == null || _volume.gameObject.scene != scene)
        {
            var go = new GameObject("Graphics Look");
            SceneManager.MoveGameObjectToScene(go, scene);
            _volume = go.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = VolumePriority;
        }

        if (_volume.sharedProfile != null)
            Object.Destroy(_volume.sharedProfile);

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Graphics Look (runtime)";

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(config.VignetteIntensity);
        vignette.smoothness.Override(config.VignetteSmoothness);

        if (tier == GraphicsTier.High)
        {
            GraphicsLookConfig.ZoneGrade grade = config.GetGrade(scene.name);

            profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.Neutral);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.contrast.Override(grade.Contrast);
            color.saturation.Override(grade.Saturation);
            color.postExposure.Override(grade.PostExposure);
            color.colorFilter.Override(grade.ColorFilter);

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(config.BloomIntensity);
            bloom.threshold.Override(config.BloomThreshold);
            bloom.scatter.Override(config.BloomScatter);
            bloom.highQualityFiltering.Override(false);
            bloom.downscale.Override(BloomDownscaleMode.Quarter);
        }

        _volume.sharedProfile = profile;
    }
}
