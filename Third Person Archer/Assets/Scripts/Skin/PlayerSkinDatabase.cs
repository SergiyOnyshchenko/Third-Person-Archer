using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkinDatabase", menuName = "Database/PlayerSkin")]
public class PlayerSkinDatabase : ScriptableObject
{
    [SerializeField] private PlayerSkinData[] _skins;

    public PlayerSkinData GetSkinByIndex(int index)
    {
        foreach (var skin in _skins)
        {
            if(skin.Index == index)
                return skin;
        }

        return null;
    }
}
