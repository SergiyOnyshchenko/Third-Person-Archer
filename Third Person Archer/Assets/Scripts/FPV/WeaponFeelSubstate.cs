using Actor;
using Lofelt.NiceVibrations;
using UnityEngine;

/// <summary>
/// Gives a weapon its own feel while its shooting state is active:
/// a click + haptic at full draw, hand tremor when the full draw is held too long,
/// a wind-up lean while drawing and a recoil kick + haptic on the shot.
/// </summary>
public class WeaponFeelSubstate : SubState, IActorIniter
{
    [SerializeField] private ProjectileShooter _shooter;

    [Header("Full draw")]
    [SerializeField] private HapticPatterns.PresetType _fullDrawHaptic = HapticPatterns.PresetType.None;
    [Tooltip("Small view dip (degrees) when the draw locks in.")]
    [SerializeField] private float _fullDrawKick;

    [Header("Tremor (holding the full draw)")]
    [Tooltip("Seconds at full draw before the hands start to shake; 0 = never.")]
    [SerializeField, Min(0f)] private float _tremorDelay;
    [Tooltip("Seconds for the tremor to reach full strength.")]
    [SerializeField, Min(0.01f)] private float _tremorRamp = 1.5f;
    [Tooltip("Tremor strength in degrees.")]
    [SerializeField, Min(0f)] private float _tremorAmplitude;
    [SerializeField, Min(0f)] private float _tremorFrequency = 2.5f;

    [Header("Wind-up")]
    [Tooltip("Camera roll (degrees) at full draw. Roll doesn't move the aim.")]
    [SerializeField] private float _windupRoll;

    [Header("Shot")]
    [Tooltip("Recoil kick, degrees up.")]
    [SerializeField, Min(0f)] private float _recoilPitch;
    [Tooltip("Random sideways recoil, ± degrees.")]
    [SerializeField, Min(0f)] private float _recoilYaw;
    [SerializeField] private HapticPatterns.PresetType _shotHaptic = HapticPatterns.PresetType.None;

    private HeadRotator _head;
    private WeaponPull _pull;
    private bool _wasFull;
    private float _fullTime;
    private float _noiseSeed;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _head)) { }
        if (actor.TryGetProperty(out _pull)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _wasFull = false;
        _fullTime = 0f;
        _noiseSeed = Random.value * 100f;

        if (_shooter != null)
            _shooter.OnShooted.AddListener(OnShot);
    }

    public override void Exit()
    {
        if (_shooter != null)
            _shooter.OnShooted.RemoveListener(OnShot);

        if (_head != null)
            _head.SetFeelOffset(Vector3.zero);

        base.Exit();
    }

    private void Update()
    {
        if (_head == null || _pull == null)
            return;

        float dt = TimeScaleUtils.AimDeltaTime;
        float pull = _pull.Value;
        bool full = pull >= 0.999f;

        if (full && !_wasFull)
        {
            _fullTime = 0f;
            HapticPatterns.PlayPreset(_fullDrawHaptic);
            if (_fullDrawKick != 0f)
                _head.AddRecoil(new Vector2(0f, -_fullDrawKick));
        }
        _wasFull = full;
        _fullTime = full ? _fullTime + dt : 0f;

        Vector3 offset = new Vector3(0f, 0f, _windupRoll * Mathf.SmoothStep(0f, 1f, pull));

        if (full && _tremorDelay > 0f && _tremorAmplitude > 0f && _fullTime > _tremorDelay)
        {
            float strength = Mathf.Clamp01((_fullTime - _tremorDelay) / _tremorRamp);
            float t = Time.unscaledTime * _tremorFrequency;
            offset.x += (Mathf.PerlinNoise(_noiseSeed, t) - 0.5f) * 2f * _tremorAmplitude * strength;
            offset.y += (Mathf.PerlinNoise(t, _noiseSeed) - 0.5f) * 2f * _tremorAmplitude * strength;
        }

        _head.SetFeelOffset(offset);
    }

    private void OnShot(Projectile projectile)
    {
        HapticPatterns.PlayPreset(_shotHaptic);

        if (_head != null && (_recoilPitch > 0f || _recoilYaw > 0f))
            _head.AddRecoil(new Vector2(Random.Range(-_recoilYaw, _recoilYaw), _recoilPitch));

        // The draw is gone: drop the lean and the tremor right away.
        _wasFull = false;
        _fullTime = 0f;
        if (_head != null)
            _head.SetFeelOffset(Vector3.zero);
    }
}
