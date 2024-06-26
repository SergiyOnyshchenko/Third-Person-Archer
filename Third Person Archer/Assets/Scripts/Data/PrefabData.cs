using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabData : Data
{
    [SerializeField] private GameObject _value;
    public GameObject Value { get => _value; }
}
