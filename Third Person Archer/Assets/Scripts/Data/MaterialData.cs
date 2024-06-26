using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialData : Data
{
    [SerializeField] private Material _value;
    public Material Value { get => _value; }
}
