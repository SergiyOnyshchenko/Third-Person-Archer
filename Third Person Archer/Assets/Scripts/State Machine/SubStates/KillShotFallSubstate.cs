using Actor;
using UnityEngine;

/// <summary>
/// The x-ray kill shot: the body goes limp the moment the projectile lands, so the kill cam's slow motion shows it
/// thrown back and falling, instead of a frozen pose that drops only when the death state starts.
/// </summary>
public class KillShotFallSubstate : SubState, IActorIniter
{
    [Tooltip("Multiplier of the ragdoll's usual hit impulse.")]
    [SerializeField, Min(0f)] private float _pushScale = 0.8f;
    [Tooltip("Multiplier of the impulse's upward part: low keeps the body from flying up in slow motion.")]
    [SerializeField, Range(0f, 1f)] private float _lift = 0.3f;
    [Tooltip("Backward tumble of the body, deg/s.")]
    [SerializeField] private float _spin = 150f;

    private RagdollControll _ragdoll;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _ragdoll)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_ragdoll != null)
            _ragdoll.MakePhysical(_pushScale, _lift, _spin);
    }
}
