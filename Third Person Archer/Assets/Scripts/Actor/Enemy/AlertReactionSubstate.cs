using System;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using Animator = Actor.Animator;
using Random = UnityEngine.Random;

/// <summary>
/// How an enemy shows it has spotted the player. The first to react in a group signals the others (if its type can),
/// an activity may bring its own reaction (an archer examining its bow just readies it), otherwise one of a few variants.
/// An empty variant keeps the controller's usual alert animation.
/// </summary>
public class AlertReactionSubstate : SubState, IActorIniter
{
    [Serializable]
    public class Variant
    {
        [Tooltip("Animator state; empty = the usual alert.")]
        public string State;
        [Min(0f)] public float Weight = 1f;
    }

    private const string AlertTrigger = "Alert";

    [SerializeField] private IdleActivitySubstate _activity;
    [SerializeField] private Variant[] _variants = Array.Empty<Variant>();
    [Tooltip("Animator state of the 'over there!' signal; empty = this type doesn't signal.")]
    [SerializeField] private string _signalState;
    [Tooltip("Only one enemy signals within this radius and time.")]
    [SerializeField] private float _signalRadius = 30f;
    [SerializeField] private float _signalCooldown = 6f;
    [SerializeField] private float _blendTime = 0.2f;

    private static readonly List<(float Time, Vector3 Position)> RecentSignals = new List<(float, Vector3)>();

    private Transform _transform;
    private Animator _animator;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        if (_animator == null)
            return;

        string reaction = PickReaction();

        if (string.IsNullOrEmpty(reaction) || !_animator.HasState(reaction))
        {
            // The usual alert starts from the idle: leave the activity first.
            if (_activity != null && _activity.InActivity && _animator.HasState(_activity.LocomotionState))
                _animator.CrossFade(_activity.LocomotionState, _blendTime);
            return;
        }

        _animator.CrossFade(reaction, _blendTime);

        // The body's alert sets its trigger a frame later; clear it so the usual alert doesn't play after this one.
        Animator animator = _animator;
        DOVirtual.DelayedCall(0.15f, () => animator.ResetTrigger(AlertTrigger), false);
    }

    private string PickReaction()
    {
        // Only if the activity is really playing (an alert can come before it started).
        string linked = _activity != null && _activity.InActivity ? _activity.Current.AlertState : null;
        if (!string.IsNullOrEmpty(linked))
            return linked;

        if (!string.IsNullOrEmpty(_signalState) && TryClaimSignal())
            return _signalState;

        float total = 0f;
        foreach (var variant in _variants)
            total += variant.Weight;

        float roll = Random.value * total;
        foreach (var variant in _variants)
        {
            roll -= variant.Weight;
            if (roll <= 0f)
                return variant.State;
        }

        return null;
    }

    /// <summary>Signals if nobody nearby just did and there is an ally around to signal to.</summary>
    private bool TryClaimSignal()
    {
        float now = Time.time;
        // Expired ones, and those from an earlier play session in the editor (no domain reload).
        RecentSignals.RemoveAll(s => now - s.Time >= _signalCooldown || s.Time > now);

        float radiusSqr = _signalRadius * _signalRadius;
        foreach (var signal in RecentSignals)
            if ((signal.Position - _transform.position).sqrMagnitude <= radiusSqr)
                return false;

        bool audience = false;
        foreach (var ally in EnemyGroupReaction.Active)
            if (ally.transform != _transform && ally.IsAlive && (ally.transform.position - _transform.position).sqrMagnitude <= radiusSqr)
            {
                audience = true;
                break;
            }

        if (!audience)
            return false;

        RecentSignals.Add((now, _transform.position));
        return true;
    }
}
