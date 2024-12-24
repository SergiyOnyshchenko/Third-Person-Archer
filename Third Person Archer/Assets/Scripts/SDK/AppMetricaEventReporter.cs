using System;
using System.Collections;
using System.Collections.Generic;
using Io.AppMetrica;
using UnityEngine;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Actor;
using System.Numerics;
using Actor.Properties;

public class AppMetricaEventReporter : MonoBehaviour
{
    public static AppMetricaEventReporter Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);  
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SendLevelStartEvent(ActorController actor)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "inventory", GetActorInventoryData(actor) },
        };

        SendEvent("level_start", parameters);
    }

    public void SendLevelWinEvent(ActorController actor)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "time", GetLevelPlaytime() },
            { "health", GetActorHealthRatio(actor) },
            { "inventory", GetActorInventoryData(actor) },
            { "ammo_count", GetActorAmmoData(actor) },
        };

        SendEvent("level_win", parameters);
    }

    public void SendLevelLostEvent(ActorController actor)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "progress", GetLevelProgress() },
            { "time", GetLevelPlaytime() },
            { "inventory", GetActorInventoryData(actor) },
            { "ammo_count", GetActorAmmoData(actor) },
        };

        SendEvent("level_lose", parameters);
    }

    public void SendElementalArrowObtained(ActorController actor, ElementalType type)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "elemental_type", type.ToString() },
            { "elemental_ammo", GetElementalArrowsData(actor) },
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "progress", GetLevelProgress() },
            { "time", GetLevelPlaytime() },
        };

        SendEvent("elemental_arrow_obtaine", parameters);
    }

    public void SendElementalArrowUsed(ActorController actor, ElementalType type)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "elemental_type", type.ToString() },
            { "elemental_ammo", GetElementalArrowsData(actor) },
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "progress", GetLevelProgress() },
            { "time", GetLevelPlaytime() },
        };

        SendEvent("elemental_arrow_use", parameters);
    }

    public void SendMainMenuOpen()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("main_menu_open", parameters);
    }

    public void SendBackToMainMenuButtonPress()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "progress", GetLevelProgress() },
            { "time", GetLevelPlaytime() }
        };

        SendEvent("back_to_main_menu_button_press", parameters);
    }

    public void SendMenuWeaponPanelOpen()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("main_menu_weapon_panel_open", parameters);
    }

    public void SendMenuSkinPanelOpen()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("main_menu_skin_panel_open", parameters);
    }

    public void SendWeaponUnlock(WeaponData data)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "weapon", data.ID },
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("weapon_unlock", parameters);
    }

    public void SendWeaponEquip(WeaponData data)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "weapon", data.ID },
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("weapon_equip", parameters);
    }

    public void SendSkinEquip(PlayerSkinData data)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "skin", data.Name },
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
        };

        SendEvent("skin_equip", parameters);
    }

    public void SendHostageDied()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level_number", GetLevelNumber() },
            { "level_index", GetLevelIndex() },
            { "progress", GetLevelProgress() },
            { "time", GetLevelPlaytime() }
        };

        SendEvent("hostage_die", parameters);
    }

    private void SendEvent(string eventName, Dictionary<string, object> parameters)
    {
        string json = JsonConvert.SerializeObject(parameters, Formatting.Indented);
        //Debug.Log(eventName + " - " + json);

        AppMetrica.ReportEvent(eventName, json);
    }

    private int GetLevelNumber()
    {
        if (LevelManager.Instance == null)
            return 0;

        return LevelManager.Instance.Database.LevelNumber;
    }

    private int GetLevelIndex()
    {
        if (LevelManager.Instance == null)
            return 0;

        return LevelManager.Instance.Database.LevelIndex;
    }

    private float GetLevelProgress()
    {
        if (EnemyManager.Instance == null)
            return 0;

        return EnemyManager.Instance.GetDeadEnemiesRatio();
    }

    private float GetLevelPlaytime()
    {
        if (GameplayTimer.Instance == null)
            return 0;

        return GameplayTimer.Instance.Timer;
    }

    private float GetActorHealthRatio(ActorController actor)
    {
        float healthRatio = 1;

        if (actor.TryGetSystem(out Health health))
            healthRatio = health.Ratio;

        return healthRatio; 
    }

    private List<string> GetActorInventoryData(ActorController actor)
    {
        List<string> inventoryIDs = new List<string>();

        if (actor.TryGetSystem(out WeaponInventory inventory))
            inventoryIDs = inventory.WeaponsData.GetCurrentWeaponsID();

        return inventoryIDs;
    }

    private Dictionary<string, int> GetActorAmmoData(ActorController actor)
    {
        Dictionary<string, int> ammoCount = new Dictionary<string, int>();

        if (actor.TryGetProperty(out BowAmmo bowAmmo))
            ammoCount.Add("bow", bowAmmo.AmmoCount);

        if (actor.TryGetProperty(out CrossbowAmmo crossbowAmmo))
            ammoCount.Add("crossbow", crossbowAmmo.AmmoCount);

        if (actor.TryGetProperty(out SpearAmmo spearAmmo))
            ammoCount.Add("spear", spearAmmo.AmmoCount);

        return ammoCount;
    }

    private Dictionary<string, int> GetElementalArrowsData(ActorController actor)
    {
        Dictionary<string, int> data = new Dictionary<string, int>();

        if (actor.TryGetSystem(out ElementalController controller))
        {
            foreach (var elementalData in controller.Database)
            {
                data.Add(elementalData.Type.ToString(), elementalData.ArrowCount);
            }
        }

        return data;
    }
} 
