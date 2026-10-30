using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// MMTimeManager that eases every timescale change instead of snapping (slow-mo in / out),
/// and lets a cinematic (kill cam) own the timescale for a while via <see cref="Override"/>.
/// Freeze frames (timescale 0) stay instant.
/// </summary>
public class SmoothTimeManager : MMTimeManager
{
    [Header("Smoothing (real seconds)")]
    [SerializeField, Min(0f)] private float _slowDownTime = 0.08f;
    [SerializeField, Min(0f)] private float _speedUpTime = 0.25f;

    /// <summary>Timescale applied as-is while set (driven per frame by a cinematic); null = feedback events.</summary>
    public static float? Override { get; set; }
    public static SmoothTimeManager Instance { get; private set; }

    private float _requested = 1f;
    private float _current = 1f;
    private float _velocity;

    public override void Initialization()
    {
        Instance = this;
        _current = NormalTimescale;
        base.Initialization();
        base.ApplyTimeScale(NormalTimescale);
    }

    protected override void Update()
    {
        base.Update();

        float target = Override ?? _requested;

        // A lerping feedback already eases (from Time.timeScale), smoothing it again would compound into a crawl.
        if (Override.HasValue || LerpTimescale || target <= 0f || _current <= 0f)
        {
            _current = target;
            _velocity = 0f;
        }
        else
        {
            float smoothTime = target < _current ? _slowDownTime : _speedUpTime;
            _current = Mathf.SmoothDamp(_current, target, ref _velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            if (Mathf.Abs(_current - target) < 0.002f)
                _current = target;
        }

        base.ApplyTimeScale(_current);
    }

    // The base class calls this with its (stacked / lerped) target; we only record it and ease toward it in Update.
    protected override void ApplyTimeScale(float newValue)
    {
        _requested = newValue;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
        Override = null;
    }
}
