using UnityEngine;

/// <summary>A part of a lazily loaded elemental effect, with the place in the projectile it was taken from.</summary>
public class ElementalFxMount : MonoBehaviour
{
    [Tooltip("Path of the parent inside the projectile, e.g. \"View/VFX\".")]
    [SerializeField] private string _parentPath;
    [SerializeField] private int _siblingIndex = -1;
    [SerializeField] private Vector3 _localPosition;
    [SerializeField] private Quaternion _localRotation = Quaternion.identity;
    [SerializeField] private Vector3 _localScale = Vector3.one;

    public void Setup(string parentPath, Transform original)
    {
        _parentPath = parentPath;
        _siblingIndex = original.GetSiblingIndex();
        _localPosition = original.localPosition;
        _localRotation = original.localRotation;
        _localScale = original.localScale;
    }

    public void Attach(Transform projectile)
    {
        Transform parent = string.IsNullOrEmpty(_parentPath) ? projectile : projectile.Find(_parentPath);
        if (parent == null)
            parent = projectile;

        transform.SetParent(parent, false);
        transform.localPosition = _localPosition;
        transform.localRotation = _localRotation;
        transform.localScale = _localScale;
        if (_siblingIndex >= 0 && _siblingIndex <= parent.childCount - 1)
            transform.SetSiblingIndex(_siblingIndex);

        // Mesh-shaped emitters wrap the model they now sit on.
        foreach (var system in GetComponentsInChildren<ParticleSystem>(true))
        {
            var shape = system.shape;
            if (shape.enabled && shape.shapeType == ParticleSystemShapeType.MeshRenderer && shape.meshRenderer == null)
                shape.meshRenderer = parent.GetComponentInParent<MeshRenderer>();
        }
    }
}
