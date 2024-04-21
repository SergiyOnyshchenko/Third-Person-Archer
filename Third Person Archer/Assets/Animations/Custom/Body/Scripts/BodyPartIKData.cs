using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{

[System.Serializable]
public class BodyPartIKData : ITransformAnimationData
{
    [SerializeField] private BodyNameIK _name;
    [SerializeField] private Vector3 _position;
    [SerializeField] private Quaternion _rotation;
    public Vector3 Position { get => _position; }
    public Quaternion Rotation { get => _rotation; }
    public BodyNameIK Name { get => _name; }

    public BodyPartIKData(BodyNameIK name, Transform pivot)
    {
        _name = name;
        _position = pivot.localPosition;
        _rotation = pivot.localRotation;
    }

    public BodyPartIKData(BodyNameIK name, Vector3 position, Quaternion rotation)
    {
        _name = name;
        _position = position;
        _rotation = rotation;
    }
}

}

