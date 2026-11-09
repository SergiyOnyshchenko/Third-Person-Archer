using UnityEngine;

/// <summary>
/// Shapes the projectile's trail when it is shot: tapered, fading, and longer and brighter the harder the draw.
/// Enemy projectiles use a warm "threat" look and a glowing head so they read from across the level.
/// </summary>
public class ProjectileTrailStyle : MonoBehaviour
{
    [SerializeField] private Projectile _projectile;
    [SerializeField] private TrailRenderer _trail;

    [Header("Shape")]
    [SerializeField] private float _width = 0.07f;
    [SerializeField] private float _time = 0.22f;
    [Header("Full draw")]
    [SerializeField] private float _fullDrawWidth = 0.11f;
    [SerializeField] private float _fullDrawTime = 0.38f;
    [Header("Colour (head → tail)")]
    [SerializeField] private Color _head = new Color(1f, 1f, 1f, 0.75f);
    [SerializeField] private Color _tail = new Color(0.85f, 0.92f, 1f, 0f);
    [SerializeField] private Color _fullDrawHead = new Color(1f, 0.97f, 0.85f, 1f);

    [Header("Glowing head (threat)")]
    [SerializeField] private Material _glowMaterial;
    [SerializeField] private Color _glowColor = new Color(1f, 0.45f, 0.15f, 0.9f);
    [SerializeField] private float _glowSize = 0.45f;

    private ParticleSystem _glow;

    private void Awake()
    {
        if (_projectile == null)
            _projectile = GetComponentInParent<Projectile>();
        if (_trail == null)
            _trail = GetComponentInChildren<TrailRenderer>(true);

        if (_projectile != null)
        {
            _projectile.OnShooted.AddListener(Apply);
            _projectile.OnHited.AddListener(HideGlow);
        }
    }

    private void OnDestroy()
    {
        if (_projectile != null)
        {
            _projectile.OnShooted.RemoveListener(Apply);
            _projectile.OnHited.RemoveListener(HideGlow);
        }
    }

    private void Apply()
    {
        float power = _projectile != null ? _projectile.Power : 1f;
        // Only a near-full draw earns the long, bright streak.
        float full = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, power));

        if (_trail != null)
        {
            _trail.Clear();
            _trail.time = Mathf.Lerp(_time, _fullDrawTime, full);
            _trail.widthMultiplier = Mathf.Lerp(_width, _fullDrawWidth, full);
            _trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.15f));

            var gradient = new Gradient();
            Color head = Color.Lerp(_head, _fullDrawHead, full);
            gradient.SetKeys(
                new[] { new GradientColorKey(head, 0f), new GradientColorKey(_tail, 1f) },
                new[] { new GradientAlphaKey(head.a, 0f), new GradientAlphaKey(head.a * 0.45f, 0.35f), new GradientAlphaKey(_tail.a, 1f) });
            _trail.colorGradient = gradient;
            _trail.emitting = true;
        }

        if (_glowMaterial != null)
            ShowGlow();
    }

    private void ShowGlow()
    {
        if (_glow == null)
        {
            var go = new GameObject("Threat Glow");
            go.transform.SetParent(transform, false);
            go.layer = gameObject.layer;

            _glow = go.AddComponent<ParticleSystem>();
            _glow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _glow.main;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = 0.12f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(_glowSize * 0.85f, _glowSize * 1.15f);
            main.startColor = _glowColor;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 6;
            main.playOnAwake = false;

            var emission = _glow.emission;
            emission.rateOverTime = 40f;

            var shape = _glow.shape;
            shape.enabled = false;

            var fade = _glow.colorOverLifetime;
            fade.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            fade.color = g;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _glowMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        _glow.Play(true);
    }

    private void HideGlow()
    {
        if (_glow != null)
            _glow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
