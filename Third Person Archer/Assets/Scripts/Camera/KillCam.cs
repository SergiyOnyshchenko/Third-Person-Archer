using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Directed camera for the last, killing shot. Bullet time slows the whole world so the projectile's
/// real speed follows <see cref="_launchSpeed"/> → <see cref="_approachSpeed"/> (works for straight and
/// ballistic projectiles alike); the camera picks one of several framings, hits with a freeze frame,
/// FOV punch and shake, lingers on the falling enemy while time ramps back, then blends home.
/// Everything runs in real (unscaled) time.
/// </summary>
[DefaultExecutionOrder(-200)] // before CinemachineBrain.LateUpdate reads the camera transform
public class KillCam : MonoBehaviour
{
    private enum Shot { Chase, Side, Reverse }

    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private int _livePriority = 100;
    [Tooltip("Geometry the camera must not go through.")]
    [SerializeField] private LayerMask _obstacles = 1;

    [Header("Bullet time: projectile speed the viewer sees (m/s, real time)")]
    [SerializeField] private float _launchSpeed = 38f;
    [SerializeField] private float _approachSpeed = 2.2f;
    [Tooltip("Distance to the target over which the projectile slows from launch to approach speed.")]
    [SerializeField] private float _slowZone = 7f;
    [SerializeField, Range(0.01f, 1f)] private float _minTimeScale = 0.03f;
    [SerializeField] private float _maxFlightTime = 4f;

    [Header("Shots")]
    [SerializeField] private float _chaseFov = 52f;
    [SerializeField] private float _closeFov = 40f;
    [Tooltip("Reverse shot (from the target back at the incoming projectile) needs at least this distance.")]
    [SerializeField] private float _reverseMinDistance = 9f;
    [SerializeField, Range(0f, 1f)] private float _reverseCutProgress = 0.45f;

    [Header("Impact")]
    [SerializeField] private float _freezeFrame = 0.07f;
    [SerializeField] private float _impactTimeScale = 0.06f;
    [Tooltip("Real seconds the camera lingers on the target while time ramps back to normal.")]
    [SerializeField] private float _aftermath = 1.3f;
    [SerializeField] private float _fovPunch = 8f;
    [SerializeField] private float _shakeAmplitude = 0.05f;
    [SerializeField] private float _shakeAngle = 1.6f;
    [SerializeField] private float _pullBackDistance = 2.6f;
    [SerializeField] private float _orbitSpeed = 14f;
    [Tooltip("After the aftermath, keep the shot until the player switches to third person (runs on), at most this long, " +
             "so the camera blends straight to the run instead of first person and then third person.")]
    [SerializeField] private Actor.CameraPOV _pov;
    [SerializeField] private float _maxHold = 2f;

    [Header("Grade")]
    [SerializeField, Range(0f, 1f)] private float _flightVignette = 0.38f;
    [SerializeField, Range(-100f, 0f)] private float _flightSaturation = -30f;
    [SerializeField, Range(0f, 1f)] private float _impactChromatic = 0.85f;
    [SerializeField] private float _gradeFadeOut = 0.6f;

    private Volume _volume;
    private Vignette _vignette;
    private ChromaticAberration _chromatic;
    private ColorAdjustments _color;

    private Shot _lastShot = Shot.Side;
    private Coroutine _routine;
    private bool _hit;
    private double _lastRealTime;
    private float _dt;

    private void Awake()
    {
        CreateVolume();
    }

    private void OnDisable()
    {
        if (_routine != null)
            Release();
    }

    public void Play(Projectile projectile, Vector3 hitPoint, Action onImpact, Action onFinished)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            Release();
        }

        _routine = StartCoroutine(Run(projectile, hitPoint, onImpact, onFinished));
    }

    private IEnumerator Run(Projectile projectile, Vector3 hitPoint, Action onImpact, Action onFinished)
    {
        Transform arrow = projectile.transform;
        Rigidbody body = projectile.GetComponent<Rigidbody>();

        Vector3 start = arrow.position;
        Vector3 direction = (hitPoint - start).sqrMagnitude > 0.0001f ? (hitPoint - start).normalized : arrow.forward;
        float distance = Mathf.Max(0.5f, Vector3.Distance(start, hitPoint));
        float side = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        Shot shot = PickShot(distance);

        _hit = false;
        _lastRealTime = Time.realtimeSinceStartupAsDouble;
        _dt = 0f;
        projectile.OnHited.AddListener(OnHit);

        _camera.transform.SetParent(null);
        _camera.Priority = _livePriority;

        // ── Flight ─────────────────────────────────────────────────────────
        Camera view = Camera.main;
        Pose pose = view != null ? new Pose(view.transform.position, view.transform.rotation) : new Pose(start, arrow.rotation);
        float fov = view != null ? view.fieldOfView : _chaseFov, roll = 0f, t = 0f, launch = -1f;
        Vector3 reverseAnchor = ReverseAnchor(hitPoint, direction, side);

        while (!_hit && projectile != null && t < _maxFlightTime)
        {
            t += _dt;

            Vector3 a = arrow.position;
            Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero;
            if (velocity.sqrMagnitude > 0.01f)
                direction = Vector3.Slerp(direction, velocity.normalized, 1f - Mathf.Exp(-12f * _dt));

            float remaining = Vector3.Distance(a, hitPoint);
            float progress = Mathf.Clamp01(1f - remaining / distance);

            // Bullet time: the world runs at whatever rate makes the projectile move at the speed we want to see.
            float worldSpeed = velocity.magnitude;
            if (launch < 0f && worldSpeed > 0.1f)
                launch = Mathf.Min(_launchSpeed, worldSpeed);

            float wanted = Mathf.Lerp(_approachSpeed, launch > 0f ? launch : _launchSpeed, Mathf.SmoothStep(0f, 1f, remaining / _slowZone));
            SetTimeScale(worldSpeed > 0.1f ? Mathf.Clamp(wanted / worldSpeed, _minTimeScale, 1f) : CurrentTimeScale());

            switch (shot)
            {
                case Shot.Chase: pose = ChasePose(a, direction, hitPoint, progress, side, out fov, out roll); break;
                case Shot.Side: pose = SidePose(a, direction, hitPoint, progress, side, out fov, out roll); break;
                case Shot.Reverse:
                    pose = progress < _reverseCutProgress
                        ? ChasePose(a, direction, hitPoint, progress, side, out fov, out roll)
                        : ReversePose(a, hitPoint, reverseAnchor, out fov, out roll);
                    break;
            }

            pose.position = KeepClear(shot == Shot.Reverse && progress >= _reverseCutProgress ? hitPoint : a, pose.position);
            Apply(pose, fov, roll);
            SetGrade(Mathf.SmoothStep(0f, 1f, progress * 1.5f), 0f);

            yield return null;
            Tick();
        }

        if (projectile != null)
            projectile.OnHited.RemoveListener(OnHit);

        // ── Impact & aftermath ─────────────────────────────────────────────
        if (_hit)
        {
            onImpact?.Invoke();

            Vector3 focus = arrow != null ? arrow.position : hitPoint;
            Vector3 focusVelocity = Vector3.zero;
            Vector3 fromFocus = pose.position - focus;
            float startDistance = Mathf.Max(0.3f, fromFocus.magnitude);
            fromFocus /= startDistance;
            float startFov = fov, startRoll = roll;
            float seed = UnityEngine.Random.value * 100f;

            for (float ta = 0f; ta < _aftermath || (ta < _aftermath + _maxHold && _pov != null && !_pov.IsThirdPerson); ta += _dt)
            {
                float k = Mathf.Min(1f, ta / _aftermath);
                SetTimeScale(ta < _freezeFrame ? 0.001f : Mathf.Lerp(_impactTimeScale, 1f, Mathf.Pow(k, 2.2f)));

                if (arrow != null)
                    focus = Vector3.SmoothDamp(focus, arrow.position, ref focusVelocity, 0.18f, Mathf.Infinity, _dt);

                float pull = EaseOut(Mathf.Clamp01((ta - _freezeFrame) / 0.8f));
                Vector3 offset = Quaternion.AngleAxis(side * _orbitSpeed * ta, Vector3.up) * fromFocus
                                 * Mathf.Lerp(startDistance, Mathf.Max(startDistance, _pullBackDistance), pull)
                                 + Vector3.up * (0.35f * pull);

                float shake = Mathf.Exp(-ta * 7f);
                Vector3 position = KeepClear(focus, focus + offset) + Noise3(seed, ta * 22f) * (_shakeAmplitude * shake);
                Quaternion look = Quaternion.LookRotation(focus - position, Vector3.up)
                                  * Quaternion.Euler(Noise3(seed + 10f, ta * 18f) * (_shakeAngle * shake));

                float punch = _fovPunch * Mathf.Exp(-Mathf.Max(0f, ta - _freezeFrame) * 9f);
                Apply(new Pose(position, look), startFov - punch, Mathf.Lerp(startRoll, 0f, pull));
                SetGrade(1f, Mathf.Exp(-ta * 5f));

                yield return null;
            Tick();
            }
        }

        // ── Back to gameplay ───────────────────────────────────────────────
        Release();
        onFinished?.Invoke();

        for (float tf = 0f; tf < _gradeFadeOut; tf += _dt)
        {
            SetGrade(1f - tf / _gradeFadeOut, 0f);
            yield return null;
            Tick();
        }

        SetGrade(0f, 0f);
    }

    private void OnHit() => _hit = true;

    // Wall clock, independent of the timescale (and of a long pause, e.g. an ad).
    private void Tick()
    {
        double now = Time.realtimeSinceStartupAsDouble;
        _dt = Mathf.Min(0.1f, (float)(now - _lastRealTime));
        _lastRealTime = now;
    }

    private void Release()
    {
        _routine = null;
        _camera.Priority = 0;
        SmoothTimeManager.Override = null;
        if (SmoothTimeManager.Instance == null)
            Time.timeScale = 1f;
    }

    // ── Shots ──────────────────────────────────────────────────────────────

    private Shot PickShot(float distance)
    {
        Shot shot;
        do
        {
            shot = (Shot)UnityEngine.Random.Range(0, 3);
        }
        while (shot == _lastShot || (shot == Shot.Reverse && distance < _reverseMinDistance));

        _lastShot = shot;
        return shot;
    }

    /// <summary>Behind the projectile, swinging out to the side and closing in as it nears the target.</summary>
    private Pose ChasePose(Vector3 arrow, Vector3 forward, Vector3 target, float progress, float side, out float fov, out float roll)
    {
        Basis(forward, out Vector3 right, out Vector3 up);
        float e = Mathf.SmoothStep(0f, 1f, progress);

        Vector3 position = arrow - forward * Mathf.Lerp(1.3f, 0.75f, e) + up * Mathf.Lerp(0.12f, 0.22f, e) + right * (side * Mathf.Lerp(0.12f, 0.75f, e));
        Vector3 look = Vector3.Lerp(arrow + forward * 3f, target, Mathf.SmoothStep(0.55f, 1f, progress) * 0.6f);

        fov = Mathf.Lerp(_chaseFov, _closeFov, e);
        roll = -side * 6f * e;
        return new Pose(position, Quaternion.LookRotation(look - position, Vector3.up));
    }

    /// <summary>Tracking alongside the projectile, sliding from behind it to just ahead of it.</summary>
    private Pose SidePose(Vector3 arrow, Vector3 forward, Vector3 target, float progress, float side, out float fov, out float roll)
    {
        Basis(forward, out Vector3 right, out Vector3 up);
        float e = Mathf.SmoothStep(0f, 1f, progress);

        Vector3 position = arrow + right * (side * Mathf.Lerp(1.1f, 1.5f, e)) + up * 0.2f - forward * Mathf.Lerp(0.7f, -0.25f, e);
        Vector3 look = Vector3.Lerp(arrow + forward * 0.4f, target, Mathf.SmoothStep(0.6f, 1f, progress) * 0.45f);

        fov = Mathf.Lerp(48f, _closeFov, e);
        roll = 0f;
        return new Pose(position, Quaternion.LookRotation(look - position, Vector3.up));
    }

    /// <summary>From beside the target, looking back at the incoming projectile.</summary>
    private Pose ReversePose(Vector3 arrow, Vector3 target, Vector3 anchor, out float fov, out float roll)
    {
        Vector3 look = Vector3.Lerp(arrow, target, 0.4f);
        fov = 50f;
        roll = 0f;
        return new Pose(anchor, Quaternion.LookRotation(look - anchor, Vector3.up));
    }

    private Vector3 ReverseAnchor(Vector3 target, Vector3 forward, float side)
    {
        Basis(forward, out Vector3 right, out _);
        return target - forward * 2.4f + right * (side * 1.6f) + Vector3.up * 0.35f;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void Apply(Pose pose, float fov, float roll)
    {
        _camera.transform.SetPositionAndRotation(pose.position, pose.rotation);
        _camera.m_Lens.FieldOfView = fov;
        _camera.m_Lens.Dutch = roll;
    }

    private Vector3 KeepClear(Vector3 from, Vector3 to)
    {
        const float radius = 0.15f;
        Vector3 ray = to - from;
        float length = ray.magnitude;
        if (length < 0.001f)
            return to;

        return Physics.SphereCast(from, radius, ray / length, out RaycastHit hit, length, _obstacles, QueryTriggerInteraction.Ignore)
            ? from + ray / length * Mathf.Max(0.2f, hit.distance)
            : to;
    }

    private static void SetTimeScale(float value)
    {
        if (SmoothTimeManager.Instance != null)
            SmoothTimeManager.Override = value;
        else
            Time.timeScale = value;
    }

    private static float CurrentTimeScale() => SmoothTimeManager.Override ?? Time.timeScale;

    private static void Basis(Vector3 forward, out Vector3 right, out Vector3 up)
    {
        right = Vector3.Cross(Vector3.up, forward);
        right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
        up = Vector3.Cross(forward, right);
    }

    private static float EaseOut(float x) => 1f - (1f - x) * (1f - x) * (1f - x);

    private static Vector3 Noise3(float seed, float time) => new Vector3(
        Mathf.PerlinNoise(seed, time) * 2f - 1f,
        Mathf.PerlinNoise(seed + 31.7f, time) * 2f - 1f,
        Mathf.PerlinNoise(seed + 77.3f, time) * 2f - 1f);

    // ── Grade ──────────────────────────────────────────────────────────────

    private void CreateVolume()
    {
        _volume = gameObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 50f;
        _volume.weight = 0f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Kill Cam (runtime)";

        _vignette = profile.Add<Vignette>(true);
        _vignette.intensity.Override(_flightVignette);
        _vignette.smoothness.Override(0.45f);

        _color = profile.Add<ColorAdjustments>(true);
        _color.saturation.Override(_flightSaturation);

        _chromatic = profile.Add<ChromaticAberration>(true);
        _chromatic.intensity.Override(0f);

        _volume.sharedProfile = profile;
    }

    private void SetGrade(float weight, float impact)
    {
        if (_volume == null)
            return;

        _volume.weight = Mathf.Clamp01(Mathf.Max(weight, impact));
        _vignette.intensity.value = Mathf.Lerp(_flightVignette, Mathf.Min(1f, _flightVignette + 0.15f), impact);
        _chromatic.intensity.value = _impactChromatic * impact;
    }
}
