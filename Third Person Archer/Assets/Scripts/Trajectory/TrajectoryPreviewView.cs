using UnityEngine;
using Actor;

public enum AimTarget
{
    None,
    Body,
    Head
}

[RequireComponent(typeof(LineRenderer))]
public sealed class TrajectoryPreviewView : Actor.System
{
    [System.Serializable]
    public sealed class Profile
    {
        public Material LineMaterial;
        public Material ImpactMaterial;
        public Color LineOutlineColor = Color.white;
        public Color ImpactOutlineColor = Color.white;
    }

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");

    [SerializeField] private LineRenderer _line;
    [SerializeField] private GameObject _impactMarkerInstance;
    [SerializeField] private MeshRenderer _impactMarkerRenderer;
    [SerializeField] private Outline _lineOutline;
    [SerializeField] private Outline _impactOutline;
    [SerializeField] private Profile _normal;
    [SerializeField] private Profile _enemy;
    [Tooltip("Aiming at a critical hitbox (head). Empty materials fall back to the enemy profile.")]
    [SerializeField] private Profile _head;

    [Header("Readiness (draw below the weapon's fire threshold)")]
    [SerializeField, Range(0f, 1f)] private float _notReadyLength = 0.45f;
    [SerializeField, Range(0f, 1f)] private float _notReadyWidth = 0.55f;
    [SerializeField, Range(0f, 1f)] private float _notReadyAlpha = 0.35f;
    [Tooltip("Width / marker punch when the draw becomes ready to fire.")]
    [SerializeField, Min(0f)] private float _readyPunch = 0.8f;
    [SerializeField, Min(0.01f)] private float _readyPunchTime = 0.25f;

    [Header("Flow")]
    [Tooltip("Dash length in metres; 0 = solid line.")]
    [SerializeField, Min(0f)] private float _dashLength = 0.6f;
    [SerializeField] private float _dashSpeed = 2.5f;
    [SerializeField, Range(0f, 1f)] private float _dashGapAlpha = 0.45f;

    [Header("Head marker")]
    [SerializeField, Min(0f)] private float _headPulse = 0.25f;
    [SerializeField, Min(0f)] private float _headPulseRate = 9f;

    private AimTarget _state;
    private bool _hasState;
    private MaterialPropertyBlock _block;
    private Texture2D _dashTexture;
    private float _baseWidth;
    private Vector3 _markerScale;
    private Color _lineColor = Color.white;
    private float _readiness;
    private float _punch;
    private float _flow;

    private void Awake()
    {
        _block = new MaterialPropertyBlock();
        _baseWidth = _line.widthMultiplier;
        if (_impactMarkerInstance != null)
            _markerScale = _impactMarkerInstance.transform.localScale;

        if (_dashLength > 0f)
        {
            _dashTexture = CreateDashTexture(_dashGapAlpha);
            _line.textureMode = LineTextureMode.Tile;
        }
    }

    private void Start()
    {
        _impactMarkerInstance.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_dashTexture != null)
            Destroy(_dashTexture);
    }

    private void Reset()
    {
        _line = GetComponent<LineRenderer>();
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
        _line.useWorldSpace = true;

        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.1f, 1f) }
        );
        _line.colorGradient = grad;
        _line.textureMode = LineTextureMode.Stretch;
        _line.widthMultiplier = 0.035f;
        _line.positionCount = 0;
    }

    public void EnsureMarker(GameObject prefab)
    {
        _impactMarkerInstance.SetActive(false);
    }

    public void Clear()
    {
        _line.positionCount = 0;
        if (_impactMarkerInstance != null) _impactMarkerInstance.SetActive(false);
        ApplyProfile(AimTarget.None);
        _readiness = 0f;
        _punch = 0f;
    }

    public void Render(in TrajectoryPrediction prediction, AimTarget target = AimTarget.None, float readiness = 1f)
    {
        if (prediction.PointCount <= 1)
        {
            Clear();
            return;
        }

        float dt = TimeScaleUtils.AimDeltaTime;

        if (readiness >= 1f && _readiness < 1f)
            _punch = 1f;
        _readiness = readiness;
        _punch = Mathf.MoveTowards(_punch, 0f, dt / _readyPunchTime);
        float punch = _punch * _punch * _readyPunch;

        ApplyProfile(target);

        float ready = Mathf.SmoothStep(0f, 1f, readiness);
        float length = Mathf.Lerp(_notReadyLength, 1f, ready);
        DrawPoints(in prediction, length);

        _line.widthMultiplier = _baseWidth * Mathf.Lerp(_notReadyWidth, 1f, ready) * (1f + punch);
        _flow += dt * _dashSpeed;
        ApplyLineBlock(Mathf.Lerp(_notReadyAlpha, 1f, ready));

        if (_impactMarkerInstance != null)
        {
            // The marker only shows when the arc is drawn all the way to it.
            if (prediction.Hit && length >= 0.999f)
            {
                float scale = 1f + punch * 0.6f;
                if (target == AimTarget.Head)
                    scale *= 1f + _headPulse * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _headPulseRate));

                _impactMarkerInstance.transform.position = prediction.HitInfo.point;
                _impactMarkerInstance.transform.rotation = Quaternion.LookRotation(prediction.HitInfo.normal);
                _impactMarkerInstance.transform.localScale = _markerScale * scale;
                _impactMarkerInstance.SetActive(true);
            }
            else
            {
                _impactMarkerInstance.SetActive(false);
            }
        }
    }

    // Draws the first `fraction` of the arc by length (the end point is interpolated, so it grows smoothly).
    private void DrawPoints(in TrajectoryPrediction prediction, float fraction)
    {
        var points = prediction.Points;
        int count = prediction.PointCount;

        if (fraction >= 0.999f)
        {
            _line.positionCount = count;
            for (int i = 0; i < count; i++)
                _line.SetPosition(i, points[i]);
            return;
        }

        float total = 0f;
        for (int i = 1; i < count; i++)
            total += Vector3.Distance(points[i - 1], points[i]);

        float wanted = total * fraction;
        float walked = 0f;
        int drawn = 1;
        _line.positionCount = count;
        _line.SetPosition(0, points[0]);

        for (int i = 1; i < count; i++)
        {
            float segment = Vector3.Distance(points[i - 1], points[i]);
            if (walked + segment >= wanted)
            {
                float t = segment > 0f ? (wanted - walked) / segment : 1f;
                _line.SetPosition(drawn++, Vector3.Lerp(points[i - 1], points[i], t));
                break;
            }

            walked += segment;
            _line.SetPosition(drawn++, points[i]);
        }

        _line.positionCount = drawn;
    }

    private void ApplyLineBlock(float alpha)
    {
        // Solid, fully opaque line: draw the material as authored.
        if (_dashTexture == null && alpha >= 0.999f)
        {
            _line.SetPropertyBlock(null);
            return;
        }

        _line.GetPropertyBlock(_block);

        Color color = _lineColor;
        color.a *= alpha;
        _block.SetColor(BaseColorId, color);

        if (_dashTexture != null)
        {
            // Tile mode maps U to world metres: 1 / dash length tiles per metre, scrolling toward the target.
            float tiling = 1f / _dashLength;
            _block.SetTexture(BaseMapId, _dashTexture);
            _block.SetVector(BaseMapStId, new Vector4(tiling, 1f, -_flow * tiling % 1f, 0f));
        }

        _line.SetPropertyBlock(_block);
    }

    private void ApplyProfile(AimTarget target)
    {
        if (_hasState && _state == target) return;
        _hasState = true;
        _state = target;

        var p = target == AimTarget.Head ? _head : target == AimTarget.Body ? _enemy : _normal;
        var fallback = target == AimTarget.None ? _normal : _enemy;

        Material lineMaterial = p != null && p.LineMaterial != null ? p.LineMaterial : fallback.LineMaterial;
        Material impactMaterial = p != null && p.ImpactMaterial != null ? p.ImpactMaterial : fallback.ImpactMaterial;
        if (p == null || p.LineMaterial == null)
            p = fallback;

        if (lineMaterial != null)
        {
            _line.sharedMaterial = lineMaterial;
            _lineColor = lineMaterial.HasProperty(BaseColorId) ? lineMaterial.GetColor(BaseColorId) : Color.white;
        }

        if (_impactMarkerRenderer != null && impactMaterial != null)
            _impactMarkerRenderer.sharedMaterial = impactMaterial;

        if (_lineOutline != null)
            _lineOutline.OutlineColor = p.LineOutlineColor;

        if (_impactOutline != null)
            _impactOutline.OutlineColor = p.ImpactOutlineColor;
    }

    private static Texture2D CreateDashTexture(float gapAlpha)
    {
        const int width = 64;
        var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            name = "Trajectory Dashes"
        };

        // Bright dash with soft edges, dimmer gap.
        for (int x = 0; x < width; x++)
        {
            float u = (x + 0.5f) / width;
            float dash = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.12f, u)) *
                         Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 0.5f, u));
            texture.SetPixel(x, 0, new Color(1f, 1f, 1f, Mathf.Lerp(gapAlpha, 1f, dash)));
        }

        texture.Apply(false, true);
        return texture;
    }
}
