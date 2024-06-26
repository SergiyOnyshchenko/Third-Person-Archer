using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialsData : Data
{
    [SerializeField] private Material[] _values;
    public Material[] Values { get => _values; }
}
