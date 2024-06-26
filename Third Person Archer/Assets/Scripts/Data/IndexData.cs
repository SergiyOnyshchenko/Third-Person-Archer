using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IndexData : Data
{
    [SerializeField] private int _value;
    public int Value { get => _value; }
}
