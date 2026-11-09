using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Projectile impacts that read what was hit: dust and chips off the ground, splinters off wood, sparks off
/// stone and metal, a flash on a body and a ring on a headshot, each with its sound. The effects are built
/// once per surface from <see cref="ImpactEffectsSettings"/> and reused.
/// </summary>
public class ImpactEffects : MonoBehaviour
{
    private sealed class Instance
    {
        public Transform Root;
        public ParticleSystem Dust, Chips, Sparks, Flash, Ring;
        public AudioSource Audio;
        public float StartedAt = -100f;
    }

    private static ImpactEffects _host;
    private static ImpactEffectsSettings _settings;
    private static bool _loaded;

    private readonly Dictionary<ImpactSurface, List<Instance>> _pools = new Dictionary<ImpactSurface, List<Instance>>();
    private readonly Dictionary<Collider, ImpactSurface> _surfaces = new Dictionary<Collider, ImpactSurface>();
    private Camera _camera;

    private static ImpactEffectsSettings Settings
    {
        get
        {
            if (!_loaded)
            {
                _settings = Resources.Load<ImpactEffectsSettings>(ImpactEffectsSettings.ResourcePath);
                _loaded = true;
            }
            return _settings;
        }
    }

    private static ImpactEffects Host
    {
        get
        {
            if (_host == null)
            {
                var go = new GameObject("Impact Effects");
                DontDestroyOnLoad(go);
                _host = go.AddComponent<ImpactEffects>();
                SceneManager.sceneUnloaded += _ => _host?._surfaces.Clear();
            }
            return _host;
        }
    }

    /// <summary>Plays the impact of a projectile that hit <paramref name="collider"/>.</summary>
    /// <param name="incoming">Flight direction at the moment of the hit.</param>
    /// <param name="strength">0..1, scales how much is thrown off.</param>
    public static void Play(Collider collider, Vector3 point, Vector3 normal, Vector3 incoming, float strength = 1f)
    {
        if (Settings == null || collider == null)
            return;

        Host.PlayAt(Host.Classify(collider, point), point, normal, incoming, strength);
    }

    public static void Play(ImpactSurface surface, Vector3 point, Vector3 normal, Vector3 incoming, float strength = 1f)
    {
        if (Settings == null)
            return;

        Host.PlayAt(surface, point, normal, incoming, strength);
    }

    private void PlayAt(ImpactSurface surface, Vector3 point, Vector3 normal, Vector3 incoming, float strength)
    {
        ImpactEffectsSettings.Preset preset = _settings.Find(surface);
        if (preset == null)
            return;

        if (_camera == null)
            _camera = Camera.main;
        if (_camera != null && (point - _camera.transform.position).sqrMagnitude > _settings.MaxDistance * _settings.MaxDistance)
            return;

        if (normal.sqrMagnitude < 0.001f)
            normal = -incoming;
        normal.Normalize();
        incoming = incoming.sqrMagnitude > 0.001f ? incoming.normalized : -normal;
        strength = Mathf.Clamp(strength, 0.3f, 1f);

        Instance fx = Take(preset);
        fx.Root.position = point + normal * 0.03f;
        fx.Root.rotation = Quaternion.LookRotation(normal);
        fx.StartedAt = Time.unscaledTime;

        Emit(fx.Dust, Mathf.RoundToInt(preset.DustCount * strength), normal);

        // Bits fly off between the normal and the glance of the projectile.
        Vector3 glance = Vector3.Reflect(incoming, normal);
        Vector3 chipsDirection = Vector3.Slerp(normal, glance, 0.45f);
        Vector3 sparksDirection = Vector3.Slerp(normal, glance, 0.7f);
        if (surface == ImpactSurface.Body || surface == ImpactSurface.Head)
            chipsDirection = Vector3.Slerp(incoming, normal, 0.35f);
        else if (surface == ImpactSurface.Release)
            chipsDirection = sparksDirection = normal;
        Emit(fx.Chips, Mathf.RoundToInt(preset.ChipCount * strength), chipsDirection);
        Emit(fx.Sparks, Mathf.RoundToInt(preset.SparkCount * strength), sparksDirection);
        Emit(fx.Flash, preset.Flash.a > 0f ? 1 : 0, normal);
        Emit(fx.Ring, preset.Ring.a > 0f ? 1 : 0, normal);

        if (fx.Audio != null && preset.Sounds != null && preset.Sounds.Length > 0)
        {
            AudioClip clip = preset.Sounds[Random.Range(0, preset.Sounds.Length)];
            if (clip != null)
            {
                fx.Audio.pitch = Random.Range(preset.Pitch.x, preset.Pitch.y);
                fx.Audio.PlayOneShot(clip, preset.Volume * Mathf.Lerp(0.7f, 1f, strength));
            }
        }
    }

    private static void Emit(ParticleSystem system, int count, Vector3 direction)
    {
        if (system == null || count <= 0)
            return;

        system.transform.rotation = Quaternion.LookRotation(direction);
        system.Emit(count);
    }

    private Instance Take(ImpactEffectsSettings.Preset preset)
    {
        if (!_pools.TryGetValue(preset.Surface, out List<Instance> pool))
            _pools[preset.Surface] = pool = new List<Instance>();

        Instance oldest = null;
        foreach (var fx in pool)
            if (oldest == null || fx.StartedAt < oldest.StartedAt)
                oldest = fx;

        // A free one is one that has finished; otherwise grow up to the budget, then reuse the oldest.
        if (oldest != null && Time.unscaledTime - oldest.StartedAt > 2.5f)
            return oldest;
        if (pool.Count < Mathf.Max(1, _settings.PoolPerSurface))
        {
            Instance created = Build(preset);
            pool.Add(created);
            return created;
        }
        return oldest;
    }

    // ── Building ─────────────────────────────────────────────────────────

    private Instance Build(ImpactEffectsSettings.Preset preset)
    {
        var root = new GameObject($"Impact {preset.Surface}").transform;
        root.SetParent(transform, false);

        var fx = new Instance { Root = root };

        if (preset.DustCount > 0 && _settings.Dust != null)
        {
            fx.Dust = CreateSystem(root, "Dust", _settings.Dust, preset.Dust,
                lifetime: new Vector2(0.45f, 0.8f), speed: new Vector2(0.4f, 1.3f), size: new Vector2(preset.DustSize * 0.7f, preset.DustSize * 1.3f),
                gravity: -0.05f, coneAngle: 40f, growth: 1.7f, drag: 2.5f);
            var sheet = fx.Dust.textureSheetAnimation;
            sheet.enabled = true;
            sheet.numTilesX = 4;
            sheet.numTilesY = 1;
            sheet.animation = ParticleSystemAnimationType.WholeSheet;
            sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            var rotation = fx.Dust.main;
            rotation.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        }

        if (preset.ChipCount > 0 && _settings.Chip != null)
        {
            fx.Chips = CreateSystem(root, "Chips", _settings.Chip, preset.Chips,
                lifetime: new Vector2(0.4f, 0.75f), speed: new Vector2(preset.ChipSpeed * 0.5f, preset.ChipSpeed * 1.2f),
                size: new Vector2(preset.ChipSize * 0.6f, preset.ChipSize * 1.3f), gravity: 1.6f, coneAngle: 35f, growth: 0.8f, drag: 0.5f);

            if (preset.Splinters)
            {
                var renderer = fx.Chips.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.velocityScale = 0.04f;
                renderer.lengthScale = 2.5f;
            }
        }

        if (preset.SparkCount > 0 && _settings.Glow != null)
        {
            fx.Sparks = CreateSystem(root, "Sparks", _settings.Glow, preset.Sparks,
                lifetime: new Vector2(0.12f, 0.3f), speed: new Vector2(4f, 9f), size: new Vector2(0.04f, 0.08f),
                gravity: 1f, coneAngle: 30f, growth: 0.5f, drag: 1f);
            var renderer = fx.Sparks.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.03f;
            renderer.lengthScale = 1.5f;
        }

        if (preset.Flash.a > 0f && _settings.Glow != null)
            fx.Flash = CreateSystem(root, "Flash", _settings.Glow, preset.Flash,
                lifetime: new Vector2(0.08f, 0.1f), speed: Vector2.zero, size: new Vector2(preset.FlashSize, preset.FlashSize * 1.15f),
                gravity: 0f, coneAngle: 0f, growth: 1.6f, drag: 0f);

        if (preset.Ring.a > 0f && _settings.Ring != null)
        {
            fx.Ring = CreateSystem(root, "Ring", _settings.Ring, preset.Ring,
                lifetime: new Vector2(0.22f, 0.22f), speed: Vector2.zero, size: new Vector2(preset.RingSize * 0.35f, preset.RingSize * 0.35f),
                gravity: 0f, coneAngle: 0f, growth: 3f, drag: 0f);
        }

        if (preset.Sounds != null && preset.Sounds.Length > 0)
        {
            fx.Audio = root.gameObject.AddComponent<AudioSource>();
            fx.Audio.playOnAwake = false;
            fx.Audio.spatialBlend = 0.75f;
            fx.Audio.minDistance = 6f;
            fx.Audio.maxDistance = 60f;
            fx.Audio.dopplerLevel = 0f;
        }

        return fx;
    }

    private static ParticleSystem CreateSystem(Transform parent, string name, Material material, Color color,
        Vector2 lifetime, Vector2 speed, Vector2 size, float gravity, float coneAngle, float growth, float drag)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var system = go.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = system.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = 40;

        var emission = system.emission;
        emission.enabled = false;

        var shape = system.shape;
        shape.enabled = coneAngle > 0f;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = 0.03f;

        var fade = system.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        var sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, growth));

        if (drag > 0f)
        {
            var limit = system.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 100f;
            limit.drag = drag;
        }

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingFudge = 0f;

        return system;
    }

    // ── Recognising the surface ─────────────────────────────────────────

    private ImpactSurface Classify(Collider collider, Vector3 point)
    {
        if (collider.TryGetComponent(out Hitbox hitbox))
            return hitbox.IsCritical ? ImpactSurface.Head : ImpactSurface.Body;

        if (collider is TerrainCollider terrain)
            return ClassifyTerrain(terrain, point);

        if (_surfaces.TryGetValue(collider, out ImpactSurface known))
            return known;

        ImpactSurface surface = ClassifyObject(collider);
        _surfaces[collider] = surface;
        return surface;
    }

    private ImpactSurface ClassifyObject(Collider collider)
    {
        var names = new System.Text.StringBuilder();
        names.Append(collider.name).Append(' ');
        if (collider.sharedMaterial != null)
            names.Append(collider.sharedMaterial.name).Append(' ');

        Renderer renderer = collider.GetComponent<Renderer>();
        if (renderer == null)
            renderer = collider.GetComponentInParent<Renderer>();
        if (renderer != null)
            foreach (var material in renderer.sharedMaterials)
                if (material != null)
                    names.Append(material.name).Append(' ');

        Transform parent = collider.transform.parent;
        for (int i = 0; i < 2 && parent != null; i++, parent = parent.parent)
            names.Append(parent.name).Append(' ');

        string text = names.ToString().ToLowerInvariant();

        if (Contains(text, _settings.Metal)) return ImpactSurface.Metal;
        if (Contains(text, _settings.Stone)) return ImpactSurface.Stone;
        if (Contains(text, _settings.Wood)) return ImpactSurface.Wood;
        if (Contains(text, _settings.Sand)) return ImpactSurface.Sand;
        if (Contains(text, _settings.Snow)) return ImpactSurface.Snow;
        if (Contains(text, _settings.Dirt)) return ZoneGround();

        return LayerMask.LayerToName(collider.gameObject.layer) == "Ground" ? ZoneGround() : ImpactSurface.Default;
    }

    private ImpactSurface ClassifyTerrain(TerrainCollider collider, Vector3 point)
    {
        TerrainData data = collider.terrainData;
        if (data == null || data.terrainLayers == null || data.terrainLayers.Length == 0)
            return ZoneGround();

        // The layer painted strongest under the hit.
        Vector3 local = point - collider.transform.position;
        int x = Mathf.Clamp(Mathf.FloorToInt(local.x / data.size.x * data.alphamapWidth), 0, data.alphamapWidth - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(local.z / data.size.z * data.alphamapHeight), 0, data.alphamapHeight - 1);
        float[,,] weights = data.GetAlphamaps(x, z, 1, 1);

        int best = 0;
        for (int i = 1; i < weights.GetLength(2); i++)
            if (weights[0, 0, i] > weights[0, 0, best])
                best = i;

        TerrainLayer layer = best < data.terrainLayers.Length ? data.terrainLayers[best] : null;
        if (layer == null)
            return ZoneGround();

        string text = layer.name.ToLowerInvariant();
        if (Contains(text, _settings.Sand)) return ImpactSurface.Sand;
        if (Contains(text, _settings.Snow)) return ImpactSurface.Snow;
        if (Contains(text, _settings.Dirt)) return ZoneGround();
        if (Contains(text, _settings.Stone)) return ImpactSurface.Stone;
        return ZoneGround();
    }

    /// <summary>Plain ground of the current zone: the Wild West (Level3) is sand, the rest is earth.</summary>
    private static ImpactSurface ZoneGround() =>
        SceneManager.GetActiveScene().name.StartsWith("Level3") ? ImpactSurface.Sand : ImpactSurface.Dirt;

    private static bool Contains(string text, string[] keys)
    {
        if (keys == null)
            return false;

        foreach (var key in keys)
            if (!string.IsNullOrEmpty(key) && text.Contains(key))
                return true;

        return false;
    }
}
