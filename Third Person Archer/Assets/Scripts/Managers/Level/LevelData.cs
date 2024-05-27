using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/Level/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private SceneReference _scene;
    public string Name { get => _name; }
    public SceneReference Scene { get => _scene; }
} 
