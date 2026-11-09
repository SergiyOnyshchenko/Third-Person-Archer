using Actor;
using Actor.Properties;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The soul of a defeated enemy: rises out of the body, then curves into the player's health bar and heals a little.
/// </summary>
public class ManaDrop : MonoBehaviour
{
    enum ManaState
    {
        Rising,
        MovingToTarget,
        Finished
    }

    [Header("Flight")]
    [Tooltip("How high the soul rises out of the body before it flies off (m).")]
    [SerializeField] private float _riseHeight = 1f;
    [SerializeField] private float _riseDuration = 0.3f;
    [Tooltip("Flight to the health bar, the same on any distance (s).")]
    [SerializeField] private float _flightDuration = 0.7f;
    [Tooltip("How far up the flight to the bar bends on the screen, as a share of the screen height (goes round the crosshair).")]
    [SerializeField] private float _screenArc = 0.15f;
    [Tooltip("Same for the flight to the chest when there is no bar on the screen (m).")]
    [SerializeField] private float _arcHeight = 4f;
    [Tooltip("Distance in front of the camera where the soul meets the bar (m).")]
    [SerializeField] private float _barDepth = 1.5f;

    [Header("Look")]
    [Tooltip("Visual size relative to the distance to the camera, so it stays small up close and readable far away.")]
    [SerializeField] private float _screenSize = 0.04f;
    [SerializeField] private float _minScale = 0.1f;
    [SerializeField] private float _maxScale = 1.2f;
    [SerializeField] private Transform _view;

    [Header("Heal")]
    [Tooltip("Share of the player's max health restored per soul.")]
    [SerializeField, Range(0f, 1f)] private float _healPercent = 0.05f;

    private ActorController _target;
    private Health _health;
    private Transform _movePoint;
    private RectTransform _barFill;
    private readonly Vector3[] _corners = new Vector3[4];
    private Camera _camera;
    private int _value = 10;
    private ManaState _state = ManaState.Rising;
    private Vector3 _start;
    private Vector3 _risePoint;
    private float _time;
    public UnityEvent OnSpawned = new UnityEvent();
    public UnityEvent OnStartMoving = new UnityEvent();
    public UnityEvent OnFinishMoving = new UnityEvent();

    public void Init(ActorController target, int value)
    {
        _target = target;
        _value = value;
        _camera = Camera.main;
        _start = transform.position;
        _risePoint = _start + Vector3.up * _riseHeight;

        if (_target.TryGetSystem(out Actor.Target moveTarget))
            _movePoint = moveTarget.TargetPoint;
        else
            _movePoint = _target.transform;

        _target.TryGetSystem(out _health);
        _barFill = FindHealthBarFill(_health);

        UpdateViewScale();
        OnSpawned?.Invoke();
    }

    private void Update()
    {
        if (_state == ManaState.Finished || _target == null)
            return;

        _time += Time.deltaTime;

        if (_state == ManaState.Rising)
        {
            float t = Mathf.Clamp01(_time / _riseDuration);
            transform.position = Vector3.LerpUnclamped(_start, _risePoint, 1f - (1f - t) * (1f - t));

            if (t >= 1f)
                StartMoving();
        }
        else
        {
            float t = Mathf.Clamp01(_time / _flightDuration);
            float eased = t * t;
            Vector3 position = TryGetBarScreenPoint(out Vector3 bar) && TryGetScreenPoint(_risePoint, out Vector3 from)
                ? FlyOnScreen(from, bar, eased)
                : Bezier(_risePoint, _risePoint + Vector3.up * _arcHeight, _movePoint.position, eased);

            if (position != transform.position)
                transform.rotation = Quaternion.LookRotation(position - transform.position);
            transform.position = position;

            if (t >= 1f)
                FinishMoving();
        }

        UpdateViewScale();
    }

    private void StartMoving()
    {
        _state = ManaState.MovingToTarget;
        _time = 0f;
        OnStartMoving?.Invoke();
    }

    private void FinishMoving()
    {
        _state = ManaState.Finished;

        if (_target.TryGetProperty(out Mana mana))
            mana.Add(_value);

        if (_health != null)
            _health.Heal(Mathf.CeilToInt(_health.MaxValue * _healPercent));

        if (_barFill != null)
            _barFill.GetComponentInParent<HealthBar>()?.PlayHealPulse();

        OnFinishMoving?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// The arc is drawn on the screen, so it bends towards the bar from any angle; the depth closes in
    /// from the soul's own distance to just in front of the camera.
    /// </summary>
    private Vector3 FlyOnScreen(Vector3 from, Vector3 to, float t)
    {
        Vector3 control = (from + to) * 0.5f + Vector3.up * (_screenArc * Screen.height);
        Vector3 screen = Bezier(from, control, to, t);
        screen.z = Mathf.Lerp(from.z, _barDepth, t);
        return _camera.ScreenToWorldPoint(screen);
    }

    private static Vector3 Bezier(Vector3 a, Vector3 control, Vector3 b, float t)
    {
        return (1f - t) * (1f - t) * a + 2f * (1f - t) * t * control + t * t * b;
    }

    private bool TryGetScreenPoint(Vector3 world, out Vector3 screen)
    {
        screen = _camera != null ? _camera.WorldToScreenPoint(world) : Vector3.zero;
        return _camera != null && screen.z > _barDepth;
    }

    /// <summary>The filled end of the health bar in screen pixels, while the bar is shown.</summary>
    private bool TryGetBarScreenPoint(out Vector3 screen)
    {
        screen = Vector3.zero;
        if (_barFill == null || !_barFill.gameObject.activeInHierarchy)
            return false;

        _barFill.GetWorldCorners(_corners);
        screen = (_corners[2] + _corners[3]) * 0.5f;
        Canvas canvas = _barFill.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            screen = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, screen);

        return true;
    }

    private void UpdateViewScale()
    {
        if (_view == null || _camera == null)
            return;

        float distance = Vector3.Distance(_camera.transform.position, transform.position);
        _view.localScale = Vector3.one * Mathf.Clamp(distance * _screenSize, _minScale, _maxScale);
    }

    /// <summary>The bar showing this health. The HUD canvas is unparented from the player at runtime, so it's looked up by its health.</summary>
    private static RectTransform FindHealthBarFill(Health health)
    {
        if (health == null)
            return null;

        // Hidden bars too: the HUD may show up while the soul is on its way.
        foreach (var bar in FindObjectsByType<HealthBar>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (bar.Health == health && bar.Slider != null && bar.Slider.fillRect != null)
                return bar.Slider.fillRect;

        return null;
    }
}
