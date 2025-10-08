using UnityEngine;
using Actor;

[RequireComponent(typeof(LineRenderer))]
public sealed class TrajectoryPreviewView : Actor.System
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private GameObject _impactMarkerInstance;

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

        // Nice default gradient: strong at start, fade at end
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
    }

    public void Render(in TrajectoryPrediction prediction)
    {
        if (prediction.PointCount <= 1)
        {
            Clear();
            return;
        }

        _line.positionCount = prediction.PointCount;
        var arr = prediction.Points;
        for (int i = 0; i < prediction.PointCount; i++)
            _line.SetPosition(i, arr[i]);

        if (_impactMarkerInstance != null)
        {
            if (prediction.Hit)
            {
                _impactMarkerInstance.transform.position = prediction.HitInfo.point;
                // Orient marker to surface normal if it has a forward axis
                _impactMarkerInstance.transform.rotation = Quaternion.LookRotation(prediction.HitInfo.normal);
                _impactMarkerInstance.SetActive(true);
            }
            else
            {
                _impactMarkerInstance.SetActive(false);
            }
        }
    }
}