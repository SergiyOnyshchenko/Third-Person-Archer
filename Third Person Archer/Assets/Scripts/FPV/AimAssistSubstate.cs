using Actor;
using UnityEngine;

/// <summary>Turns the aim assist on with this weapon's strength while its shooting state is active.</summary>
public class AimAssistSubstate : SubState, IActorIniter
{
    [Tooltip("0 = off, 1 = default. Snappy weapons get more help, precise ones less.")]
    [SerializeField, Min(0f)] private float _strength = 1f;
    private AimAssist _assist;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _assist)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_assist != null)
            _assist.Strength = _strength;
    }

    public override void Exit()
    {
        if (_assist != null)
        {
            _assist.Strength = 0f;
            _assist.Release();
        }

        base.Exit();
    }
}
