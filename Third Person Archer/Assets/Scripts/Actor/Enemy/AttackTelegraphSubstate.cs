using Actor;
using UnityEngine;

/// <summary>
/// Warns the player that this enemy is about to attack: a glint at the weapon grows from faint to blazing
/// until the shot, a cue sound plays as the wind-up starts and the off-screen arrow flashes.
/// Killing or staggering the enemy before the shot leaves the attack state, which cancels both the shot and the warning.
/// The ramp learns how long this enemy's wind-up really takes, so the glint peaks with the shot.
/// </summary>
public class AttackTelegraphSubstate : SubState, IActorIniter
{
    [SerializeField] private AnimationAttackState _attack;
    [Tooltip("Where the glint shines (bow, gun, blade). Empty = the attack's shoot point, else the head.")]
    [SerializeField] private Transform _anchor;
    [SerializeField] private Material _glintMaterial;

    [Header("Timing")]
    [Tooltip("Expected seconds from the start of the wind-up to the shot, until the real one has been measured.")]
    [SerializeField, Min(0.1f)] private float _rampTime = 1f;
    [Tooltip("The state keeps firing (e.g. a crouched rifleman): wind up again after each shot. Otherwise the glint stays off until the next attack.")]
    [SerializeField] private bool _repeatAfterShot;

    [Header("Look")]
    [Tooltip("Glint size per metre of distance to the camera, so it reads the same near and far.")]
    [SerializeField] private float _sizePerMetre = 0.3f;
    [SerializeField] private float _minSize = 1.5f;
    // Additive: near-white reads against sky and stone, saturated red disappears.
    [SerializeField] private Color _startColor = new Color(1f, 0.95f, 0.85f, 0.55f);
    [SerializeField] private Color _endColor = new Color(1f, 0.8f, 0.55f, 1f);
    [Tooltip("The glint is pulled this far towards the camera so the body doesn't hide it.")]
    [SerializeField] private float _towardsCamera = 1f;

    [Header("Sound")]
    [SerializeField] private AudioClip _cue;
    [SerializeField, Range(0f, 1f)] private float _cueVolume = 0.6f;
    [SerializeField] private float _cuePitch = 1f;

    private const float FlashTime = 0.18f;

    private Transform _actorRoot;
    private Transform _head;
    private global::Target _offscreenTarget;
    private Camera _camera;

    private ParticleSystem _glint;
    private ParticleSystem.Particle[] _particle = new ParticleSystem.Particle[1];
    private AudioSource _audio;

    private float _elapsed;
    private float _flash = -1f;
    private float _learnedRamp;
    private bool _done;

    public void InitActor(ActorController actor)
    {
        _actorRoot = actor.transform;
        _offscreenTarget = actor.GetComponentInChildren<global::Target>(true);

        foreach (var hitbox in actor.GetComponentsInChildren<Hitbox>(true))
            if (hitbox.IsCritical)
                _head = hitbox.transform;

        if (_attack == null)
            _attack = GetComponentInParent<AnimationAttackState>();

        if (_anchor == null && _attack != null && _attack.TryGetComponent(out Shooter shooter))
            _anchor = shooter.ShootPoint;
    }

    public override void Enter()
    {
        base.Enter();

        _camera = Camera.main;
        _flash = -1f;
        _done = false;
        StartCycle();

        if (_attack != null)
            _attack.OnAttacked.AddListener(OnShot);
    }

    public override void Exit()
    {
        if (_attack != null)
            _attack.OnAttacked.RemoveListener(OnShot);

        if (_glint != null)
            _glint.Clear();

        if (_offscreenTarget != null)
            _offscreenTarget.Threat = 0f;

        base.Exit();
    }

    private void OnDestroy()
    {
        if (_glint != null)
            Destroy(_glint.gameObject);
    }

    private void StartCycle()
    {
        _elapsed = 0f;

        if (_cue != null)
        {
            EnsureAudio();
            _audio.pitch = _cuePitch;
            _audio.PlayOneShot(_cue, _cueVolume);
        }
    }

    private void OnShot()
    {
        // Several Attack events in one swing (a volley): only the first one closes the wind-up.
        if (_flash >= 0f || _done)
            return;

        _learnedRamp = _learnedRamp > 0f ? Mathf.Lerp(_learnedRamp, _elapsed, 0.5f) : _elapsed;
        _flash = 0f;
    }

    private void Update()
    {
        if (_done)
            return;

        float dt = Time.deltaTime;
        float ramp = _learnedRamp > 0f ? _learnedRamp : _rampTime;
        float threat;
        float size;
        Color color;

        if (_flash >= 0f)
        {
            // The shot: a short white burst that fades out, then the next wind-up (if the state keeps attacking).
            _flash += dt;
            float t = _flash / FlashTime;
            threat = 0f;
            size = 1.6f - 0.6f * t;
            color = Color.Lerp(Color.white, _endColor, t);
            color.a = 1f - t;

            if (t >= 1f)
            {
                _flash = -1f;
                if (_repeatAfterShot)
                {
                    StartCycle();
                }
                else
                {
                    _done = true;
                    _glint.Clear();
                    return;
                }
            }
        }
        else
        {
            _elapsed += dt;
            threat = Mathf.Clamp01(_elapsed / ramp);
            float eased = threat * threat;
            // The last part of the wind-up throbs, so "now" is readable, not just "soon".
            float throb = threat > 0.7f ? 0.85f + 0.15f * Mathf.Sin(_elapsed * 40f) : 1f;
            size = Mathf.Lerp(0.45f, 1f, eased) * throb;
            color = Color.Lerp(_startColor, _endColor, eased);
        }

        if (_offscreenTarget != null)
            _offscreenTarget.Threat = threat;

        DrawGlint(size, color);
    }

    private void DrawGlint(float size, Color color)
    {
        Transform anchor = _anchor != null ? _anchor : _head;
        if (anchor == null || _glintMaterial == null)
            return;

        if (_camera == null)
            _camera = Camera.main;
        if (_camera == null)
            return;

        EnsureGlint();

        Vector3 position = anchor.position;
        Vector3 toCamera = _camera.transform.position - position;
        float distance = toCamera.magnitude;
        position += toCamera / Mathf.Max(distance, 0.01f) * Mathf.Min(_towardsCamera, distance * 0.5f);

        _particle[0].position = position;
        _particle[0].startSize = Mathf.Max(_minSize, distance * _sizePerMetre) * size;
        _particle[0].startColor = color;
        _particle[0].remainingLifetime = 10f;
        _particle[0].startLifetime = 10f;
        _particle[0].rotation = 0f;
        _glint.SetParticles(_particle, 1);
    }

    private void EnsureGlint()
    {
        if (_glint != null)
            return;

        var go = new GameObject("Attack Glint");
        go.transform.SetParent(_actorRoot, false);

        _glint = go.AddComponent<ParticleSystem>();
        _glint.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _glint.main;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;

        var emission = _glint.emission;
        emission.enabled = false;
        var shape = _glint.shape;
        shape.enabled = false;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = _glintMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.maxParticleSize = 10f;

        _glint.Play();
    }

    private void EnsureAudio()
    {
        if (_audio != null)
            return;

        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        // Mostly 2D: the cue has to be heard from across the level, but still hint at the side it comes from.
        _audio.spatialBlend = 0.35f;
        _audio.dopplerLevel = 0f;
    }
}
