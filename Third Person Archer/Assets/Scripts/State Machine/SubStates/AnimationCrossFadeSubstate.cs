using Actor;
using UnityEngine;

/// <summary>
/// Blends the animator into a state on enter, e.g. back to idle when an attack is cut short.
/// </summary>
public class AnimationCrossFadeSubstate : SubState, IActorIniter
{
    [SerializeField] private string _stateName = "Movement";
    [SerializeField, Min(0f)] private float _duration = 0.2f;
    [Tooltip("Triggers to clear first, so an attack that was queued doesn't start again.")]
    [SerializeField] private string[] _resetTriggers;

    private Actor.Animator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Actor.Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        if (_animator == null)
            return;

        if (_resetTriggers != null)
            foreach (var trigger in _resetTriggers)
                _animator.ResetTrigger(trigger);

        if (_animator.HasState(_stateName))
            _animator.CrossFade(_stateName, _duration);
    }
}
