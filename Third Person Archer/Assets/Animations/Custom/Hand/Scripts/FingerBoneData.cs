using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;

namespace CustomAnimation.Hand
{

[System.Serializable]
public class FingerBoneData : IFingerBone, IRotateAnimationData
{
    [SerializeField] private FingerName _fingerName;
    [SerializeField] private int _index;
    [SerializeField] private Quaternion _rotation; 
    public FingerName FingerName { get => _fingerName; }
    public int Index { get => _index; }
    public Quaternion Rotation { get => _rotation; }    

    public FingerBoneData(FingerName fingerName, int index, Quaternion rotation)
    {
        _fingerName = fingerName;
        _index = index;
        _rotation = rotation;
    }

    public void Inverce()
    {
        _rotation = Quaternion.Inverse(_rotation);
    }
}

}
