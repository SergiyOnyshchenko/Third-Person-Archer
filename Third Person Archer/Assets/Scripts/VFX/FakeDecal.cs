using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeDecal : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private bool _randomizedRotation = true;
    [SerializeField] private bool _randomizedScale = true;
    [SerializeField] private LayerMask _mask;

    private void Start()
    {
        if (_randomizedRotation)
            RandomizeRotation();

        if (_randomizedScale)
            RandomizeScale();

        ProjectOnPlane();
    }

    public void RandomizeRotation()
    {
        float randomAngle = Random.Range(0, 360);

        Vector3 angles = _sprite.transform.localEulerAngles;
        angles.y = randomAngle;
        _sprite.transform.localEulerAngles = angles;
    }

    public void RandomizeScale()
    {
        float randomScale = Random.Range(0.75f, 1.25f);
        _sprite.transform.localScale *= randomScale;
    }

    private void ProjectOnPlane() 
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + transform.up * 1, -transform.up, out hit, 2, _mask))
        {
            _sprite.transform.position = hit.point + (transform.up * 0.02f);
        }
    }
}
