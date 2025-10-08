using UnityEngine;

[ExecuteAlways]
public sealed class CameraIndependentScale : MonoBehaviour
{
    public enum Mode { WorldSize, ScreenSize }

    [Header("General")]
    [SerializeField] private Mode _mode = Mode.WorldSize;
    [SerializeField] private bool _affectX = true;
    [SerializeField] private bool _affectY = true;
    [SerializeField] private bool _affectZ = true;

    [Header("WorldSize mode")]
    [SerializeField] private Vector3 _targetWorldScale = Vector3.one;

    [Header("ScreenSize mode")]
    [SerializeField, Min(0.0001f)] private float _referenceDistance = 10f;
    [SerializeField] private Vector3 _referenceLocalScale = Vector3.one;
    [SerializeField, Min(0f)] private float _maxScaleMultiplier = 100f;

    // Cache for play mode; editor path will fall back to the Transform property.
    private Transform _tr;
    private Camera _camera;

    // Always use this accessor; it’s safe in edit-time and play mode.
    private Transform Tr => _tr != null ? _tr : transform;

    private void Awake()
    {
        _tr = transform;
        _camera = Camera.main;

        if (_targetWorldScale == Vector3.zero)
        {
            _targetWorldScale = GetGlobalScale(Tr);
            if (_targetWorldScale == Vector3.zero) _targetWorldScale = Vector3.one;
        }

        if (_referenceLocalScale == Vector3.zero)
        {
            _referenceLocalScale = Tr.localScale == Vector3.zero ? Vector3.one : Tr.localScale;
        }
    }

    private void OnEnable()
    {
        _tr = transform;
        if (_camera == null) _camera = Camera.main;
        ApplyScaleSafe();
    }

    private void LateUpdate()
    {
        if (_camera == null) _camera = Camera.main;
        ApplyScaleSafe();
    }

    private void ApplyScaleSafe()
    {
        // Extra guard to avoid editor-time NREs on prefab/serialization changes.
        if (Tr == null) return;

        switch (_mode)
        {
            case Mode.WorldSize:
                ApplyWorldSize();
                break;
            case Mode.ScreenSize:
                ApplyScreenSize();
                break;
        }
    }

    private void ApplyWorldSize()
    {
        Vector3 parentLossy = GetParentLossyScale(Tr);
        Vector3 desiredLocal = DivideSafe(_targetWorldScale, parentLossy);
        SetLocalScaleAxes(desiredLocal);
    }

    private void ApplyScreenSize()
    {
        if (_camera == null) _camera = Camera.main;

        if (_camera != null && !_camera.orthographic)
        {
            float distance = Vector3.Distance(_camera.transform.position, Tr.position);
            if (distance <= 0.0001f) distance = 0.0001f;

            float tanHalfFov = Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
            float refFactor = _referenceDistance * tanHalfFov;
            float curFactor = distance * tanHalfFov;

            float multiplier = refFactor <= 0.0001f ? 1f : (curFactor / refFactor);
            if (_maxScaleMultiplier > 0f) multiplier = Mathf.Min(multiplier, _maxScaleMultiplier);

            Vector3 desiredLocal = _referenceLocalScale * multiplier;
            SetLocalScaleAxes(desiredLocal);
        }
        else
        {
            SetLocalScaleAxes(_referenceLocalScale);
        }
    }

    // ----------------- Utility -----------------

    private static Vector3 GetGlobalScale(Transform t) => t != null ? t.lossyScale : Vector3.one;

    private static Vector3 GetParentLossyScale(Transform t)
    {
        if (t == null) return Vector3.one;
        var p = t.parent;
        if (p == null) return Vector3.one;

        Vector3 s = p.lossyScale;
        if (ApproximatelyZero(s.x)) s.x = 1f;
        if (ApproximatelyZero(s.y)) s.y = 1f;
        if (ApproximatelyZero(s.z)) s.z = 1f;
        return s;
    }

    private static Vector3 DivideSafe(in Vector3 a, in Vector3 b)
    {
        return new Vector3(
            b.x == 0f ? a.x : a.x / b.x,
            b.y == 0f ? a.y : a.y / b.y,
            b.z == 0f ? a.z : a.z / b.z
        );
    }

    private static bool ApproximatelyZero(float v) => Mathf.Abs(v) < 1e-6f;

    private void SetLocalScaleAxes(Vector3 target)
    {
        Vector3 current = Tr.localScale;
        if (_affectX) current.x = target.x;
        if (_affectY) current.y = target.y;
        if (_affectZ) current.z = target.z;
        Tr.localScale = current;
    }
}