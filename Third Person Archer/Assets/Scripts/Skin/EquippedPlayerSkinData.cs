using System;
using System.Collections;
using System.Collections.Generic;
using Playgama;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EquippedPlayerSkin", menuName = "Data/Skins/EquippedPlayerSkin")]
public class EquippedPlayerSkinData : ScriptableObject
{
    [SerializeField] private PlayerSkinData _skinData;
    private const string SkinKey = "EquippedPlayerSkin";
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
        Bridge.storage.Set(SkinKey, _skinData.Index.ToString(), null);
    }

    public void Load(UnityAction<int> onLoaded)
    {
        Bridge.storage.Get(SkinKey, (success, value) =>
        {
            if (success && int.TryParse(value, out var index))
            {
                onLoaded?.Invoke(index);
            }
            else
            {
                onLoaded?.Invoke(0);
            }
        });
    }
}
