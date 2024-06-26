using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkin", menuName = "Data/Skins/PlayerSkinData")]
public class PlayerSkinData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _index;
    public string Name { get => _name; }
    public int Index { get => _index; }
}
