using System.Collections.Generic;
using Actor;
using UnityEngine;

/// <summary>
/// How an enemy reacts to its group: when it notices something it starts and looks that way before it reacts
/// (the delay is the perception's reaction time), and when an ally dies close by it jumps and is alarmed,
/// or only flinches if it is already fighting.
/// </summary>
public class EnemyGroupReaction : MonoBehaviour, IActorIniter
{
    [Tooltip("Allies dying within this distance are noticed.")]
    [SerializeField] private float _allyRadius = 12f;
    [Tooltip("Seconds the upper body keeps looking at what startled it.")]
    [SerializeField] private float _lookHold = 0.8f;
    [Tooltip("Strength of the flinch when an ally dies while this enemy is already fighting.")]
    [SerializeField, Range(0f, 1f)] private float _busyStrength = 0.4f;

    private static readonly List<EnemyGroupReaction> All = new List<EnemyGroupReaction>();

    private ActorController _actor;
    private Health _health;
    private PerceptionInput _perception;
    private HitFlinch _flinch;

    public void InitActor(ActorController actor)
    {
        _actor = actor;
        _flinch = actor.GetComponentInChildren<HitFlinch>(true);

        if (actor.TryGetSystem(out _health))
            _health.OnHealthZero.AddListener(OnDied);

        if (actor.TryGetInput(out _perception))
            _perception.OnNoticed.AddListener(Startle);
    }

    private void OnEnable() => All.Add(this);

    private void OnDisable() => All.Remove(this);

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnHealthZero.RemoveListener(OnDied);

        if (_perception != null)
            _perception.OnNoticed.RemoveListener(Startle);
    }

    private void Startle(Vector3 source)
    {
        if (_flinch != null)
            _flinch.Startle(source, _lookHold);
    }

    private void Flinch(Vector3 source)
    {
        if (_flinch != null)
            _flinch.Startle(source, _lookHold * 0.5f, _busyStrength);
    }

    private void OnDied()
    {
        Vector3 position = _actor.transform.position;
        float radiusSqr = _allyRadius * _allyRadius;

        // A copy: an ally's reaction may change the list.
        foreach (var ally in All.ToArray())
            if (ally != this && ally._actor != null && (ally._actor.transform.position - position).sqrMagnitude <= radiusSqr)
                ally.OnAllyDied(_actor.gameObject);
    }

    private void OnAllyDied(GameObject ally)
    {
        if (_actor.IsDead || (_health != null && _health.IsDead) || _perception == null || !_perception.IsActive)
            return;

        if (_perception.Target == null)
            _perception.ReciveSound("AllyDeath", 1f, ally);
        else
            Flinch(ally.transform.position);
    }
}
