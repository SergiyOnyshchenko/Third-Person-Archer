using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class EnemyStatsReceiverSystem : Actor.System, IEnemyStatsReceiver, IActorIniter
{
    private MaxHealth _maxHealth;
    private Damage _damage;
    private DeathCost _deathCost;
    private ManaDropper _manaDropper;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _maxHealth)) { }
        if (actor.TryGetProperty(out _damage)) { }
        if (actor.TryGetProperty(out _deathCost)) { }
        if (actor.TryGetSystem(out _manaDropper)) { }
    }

    public void SetHealth(int value)
    {
        if (_maxHealth == null)
            return;

        _maxHealth.SetValue(value);
    }

    public void SetDamage(int value)
    {
        if (_damage == null)
            return;

        _damage.SetValue(value);
    }

    public void SetCoinsDrop(int value)
    {
        if (_deathCost == null)
            return;

        _deathCost.SetValue(value);
    }

    public void SetManaDrop(int value)
    {
        if (_manaDropper == null)
            return;

        _manaDropper.SetManaDropAmount(value);
    }
}
