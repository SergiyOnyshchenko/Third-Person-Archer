using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A melee enemy closing in breaks into a sprint for the last few metres, so its attack is announced from afar.
/// </summary>
public class MeleeChargeSubstate : SubState, IActorIniter
{
    [Tooltip("Distance to the target (m) at which the enemy starts sprinting.")]
    [SerializeField] private float _chargeDistance = 7f;
    [SerializeField] private float _chargeSpeed = 4.2f;
    [SerializeField] private float _chargeAcceleration = 10f;

    private Transform _transform;
    private PerceptionInput _perception;
    private NavMeshAgent _agent;
    private Speed _speed;
    private Acceleration _acceleration;
    private bool _charging;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;
        _agent = actor.GetComponent<NavMeshAgent>();

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;

        if (actor.TryGetProperty(out Speed speed))
            _speed = speed;

        if (actor.TryGetProperty(out Acceleration acceleration))
            _acceleration = acceleration;
    }

    public override void Enter()
    {
        base.Enter();
        _charging = false;
    }

    public override void Exit()
    {
        if (_charging)
        {
            _speed?.ResetValue();
            _acceleration?.ResetValue();
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                if (_speed != null)
                    _agent.speed = _speed.Value;
                // Brakes out of the sprint as hard as it got into it; the next path restores the normal acceleration.
                _agent.acceleration = _chargeAcceleration;
            }
            _charging = false;
        }

        base.Exit();
    }

    private void Update()
    {
        if (_charging || _perception == null || _perception.Target == null || _agent == null)
            return;

        float distance = Vector3.Distance(_perception.Target.RootPoint.position, _transform.position);
        if (distance > _chargeDistance)
            return;

        _charging = true;

        // The properties too: the mover re-reads them every frame (acceleration) and on every new path (speed).
        _speed?.SetValue(_chargeSpeed);
        _acceleration?.SetValue(_chargeAcceleration);
        _agent.speed = _chargeSpeed;
        _agent.acceleration = _chargeAcceleration;
    }
}
