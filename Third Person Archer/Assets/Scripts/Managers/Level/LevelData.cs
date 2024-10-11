using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/Level/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private SceneReference _scene;
    [Space]
    [SerializeField] private LevelSequenceData _sequence;
    [SerializeField] private bool _isBoss;
    [Space]
    [SerializeField] private float _manaMultiplier = 1;
    public string Name { get => _name; }
    public SceneReference Scene { get => _scene; }
    public LevelSequenceData Sequence { get => _sequence; }
    public bool IsBoss { get => _isBoss; }
    public float ManaMultiplier { get => _manaMultiplier; }
} 
