using System;
using Actor;
using DG.Tweening;
using UnityEngine;
using Animator = Actor.Animator;
using Random = UnityEngine.Random;

/// <summary>
/// What an unaware enemy is busy with before it notices the player: looking around, examining its bow, talking to
/// an ally, praying... One activity is picked per enemy, so a group doesn't move in sync.
/// The <see cref="AlertReactionSubstate"/> reads the activity to react in a fitting way.
/// </summary>
public class IdleActivitySubstate : SubState, IActorIniter
{
    [Serializable]
    public class Activity
    {
        [Tooltip("Animator state to blend into; empty = the controller's own idle.")]
        public string State;
        [Min(0f)] public float Weight = 1f;
        [Tooltip("Only when another enemy stands close (talking).")]
        public bool NeedsCompany;
        [Tooltip("Animator state played when the enemy notices the player during this activity; empty = the usual alert.")]
        public string AlertState;
    }

    [SerializeField] private Activity[] _activities = Array.Empty<Activity>();
    [Tooltip("Animator state the enemy returns to when it leaves an activity without an alert of its own.")]
    [SerializeField] private string _locomotionState = "Movement";
    [SerializeField] private float _companyRadius = 4f;
    [SerializeField] private float _blendTime = 0.4f;
    [Tooltip("Seconds, random up to this, before the activity starts.")]
    [SerializeField] private float _maxStartDelay = 0.4f;

    private Transform _transform;
    private Animator _animator;
    private PerceptionInput _perception;
    private Tween _start;
    private float _initTime;

    /// <summary>The activity picked on the last enter; null before.</summary>
    public Activity Current { get; private set; }
    public string LocomotionState => _locomotionState;

    /// <summary>The animator is in (or blending into) the current activity's own state.</summary>
    public bool InActivity => Current != null && !string.IsNullOrEmpty(Current.State) && _animator != null && _animator.IsInState(Current.State);

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;
        _initTime = Time.time;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;
    }

    public override void Enter()
    {
        base.Enter();

        Current = Pick();
        if (Current == null || string.IsNullOrEmpty(Current.State) || _animator == null)
            return;

        // Somewhere into the loop, so neighbours doing the same thing are out of step.
        float offset = Random.Range(0f, 2f);

        // Just switched on (the stage started): already busy, not visibly starting.
        bool instant = Time.time - _initTime < 0.5f;

        // Never on the enter frame: an animator switched on this frame isn't ready and would drop the blend.
        _start = DOVirtual.DelayedCall(instant ? 0f : Random.Range(0f, _maxStartDelay), () =>
        {
            _start = null;
            if (_animator.HasState(Current.State))
                _animator.CrossFade(Current.State, instant ? 0f : _blendTime, 0, offset);
        }, false);
    }

    public override void Exit()
    {
        _start?.Kill();
        _start = null;

        // Leaving for anything but an alert (a patrol): just go back to the idle. The alert reaction handles the rest.
        bool alerted = _perception != null && _perception.Target != null;
        if (!alerted && InActivity && _animator.HasState(_locomotionState))
            _animator.CrossFade(_locomotionState, _blendTime);

        base.Exit();
    }

    private Activity Pick()
    {
        bool company = HasCompany();
        float total = 0f;

        foreach (var activity in _activities)
            if (company || !activity.NeedsCompany)
                total += activity.Weight;

        float roll = Random.value * total;
        foreach (var activity in _activities)
        {
            if (!company && activity.NeedsCompany)
                continue;

            roll -= activity.Weight;
            if (roll <= 0f)
                return activity;
        }

        return null;
    }

    private bool HasCompany()
    {
        float radiusSqr = _companyRadius * _companyRadius;

        foreach (var ally in EnemyGroupReaction.Active)
            if (ally.transform != _transform && (ally.transform.position - _transform.position).sqrMagnitude <= radiusSqr)
                return true;

        return false;
    }
}
