using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class BossMaxHealthApplier : MonoBehaviour
{

    private void Start()
    {
        var player = FindObjectOfType<Player>();
        
        if (player == null)
            return;

        var boss = GetComponent<ActorController>();

        if (boss == null)
            return;

        if(player.TryGetProperty(out Damage damage) && boss.TryGetProperty(out MaxHealth maxHealth))
        {
            int health = damage.Value * 10;
            Debug.Log("Set Health: Damage " + damage.Value + " " + health);
            maxHealth.SetValue(health);
        }
    }
}
