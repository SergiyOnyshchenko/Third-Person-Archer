using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/Level/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private SceneReference _scene;
    public SceneReference Scene { get => _scene; }
} 
