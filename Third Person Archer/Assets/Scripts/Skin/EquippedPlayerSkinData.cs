using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquippedPlayerSkin", menuName = "Data/Skins/EquippedPlayerSkin")]
public class EquippedPlayerSkinData : ScriptableObject
{
    [SerializeField] private PlayerSkinData _skinData;
    public PlayerSkinData SkinData { get => _skinData; }
    public event Action<PlayerSkinData> OnEquipped;

    public void Equip(PlayerSkinData skinData)
    {
        _skinData = skinData;
        OnEquipped?.Invoke(skinData);

        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt("EquippedPlayerSkin", _skinData.Index);
    }

    public int Load() 
    {
        return PlayerPrefs.GetInt("EquippedPlayerSkin", 0);
    }
}
