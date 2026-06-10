using UnityEngine;
using Actor;

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

    [SerializeField] private LineRenderer _line;
    [SerializeField] private GameObject _impactMarkerInstance;
    [SerializeField] private MeshRenderer _impactMarkerRenderer;
    [SerializeField] private Outline _lineOutline;
    [SerializeField] private Outline _impactOutline;
    [SerializeField] private Profile _normal;
    [SerializeField] private Profile _enemy;

    private bool _isEnemyState;

    private void Start()
    {
        _impactMarkerInstance.SetActive(false);
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
        ApplyProfile(false);
    }

    public void Render(in TrajectoryPrediction prediction, bool targetsEnemy = false)
    {
        if (prediction.PointCount <= 1)
        {
            Clear();
            return;
        }

        ApplyProfile(targetsEnemy);

        _line.positionCount = prediction.PointCount;
        var arr = prediction.Points;
        for (int i = 0; i < prediction.PointCount; i++)
            _line.SetPosition(i, arr[i]);

        if (_impactMarkerInstance != null)
        {
            if (prediction.Hit)
            {
                _impactMarkerInstance.transform.position = prediction.HitInfo.point;
                _impactMarkerInstance.transform.rotation = Quaternion.LookRotation(prediction.HitInfo.normal);
                _impactMarkerInstance.SetActive(true);
            }
            else
            {
                _impactMarkerInstance.SetActive(false);
            }
        }
    }

    private void ApplyProfile(bool enemyState)
    {
        if (_isEnemyState == enemyState) return;
        _isEnemyState = enemyState;

        var p = enemyState ? _enemy : _normal;

        if (p.LineMaterial != null)
            _line.sharedMaterial = p.LineMaterial;

        if (_impactMarkerRenderer != null && p.ImpactMaterial != null)
            _impactMarkerRenderer.sharedMaterial = p.ImpactMaterial;

        if (_lineOutline != null)
            _lineOutline.OutlineColor = p.LineOutlineColor;

        if (_impactOutline != null)
            _impactOutline.OutlineColor = p.ImpactOutlineColor;
    }
}
