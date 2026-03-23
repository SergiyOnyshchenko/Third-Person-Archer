using UnityEngine;

public class XRayLayer : MonoBehaviour, IXRayInstance
{
    private const int _xrayLayer = 14;
    private int _originalLayer;

    private void Awake()
    {
        _originalLayer = gameObject.layer;
    }

    public void ActivateXRay(bool value)
    {
        int targetLayer = value ? _xrayLayer : _originalLayer;
        SetLayerRecursive(gameObject, targetLayer);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}