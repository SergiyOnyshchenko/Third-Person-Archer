using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinIniter : MonoBehaviour
{
    [SerializeField] private PlayerSkinDatabase _database;
    [SerializeField] private EquippedPlayerSkinData _equippedData;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _equippedData.Load((int playerSkinDataIndex) =>
        {
            PlayerSkinData skinData = _database.GetSkinByIndex(playerSkinDataIndex);
            _equippedData.Equip(skinData);
        });
    }
}
