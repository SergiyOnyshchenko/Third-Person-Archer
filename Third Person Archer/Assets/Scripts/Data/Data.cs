using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data 
{
    [SerializeField] private string _label;
    public string Label => _label;
    public void SetLabel(string label) => _label = label;
}
