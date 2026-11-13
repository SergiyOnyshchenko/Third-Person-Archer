using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.AI;
using Animator = Actor.Animator;

/// <summary>
/// Between attacks a melee enemy doesn't stand still: it keeps facing the target and either sidesteps around it
/// or taunts (shield taps, arms wide...). It always stays within striking range.
/// </summary>
public class MeleeWaitSubstate : SubState, IActorIniter
{
    private const string TauntTrigger = "Taunt";
    private const string TauntTypeParam = "TauntType";

    [Tooltip("Seconds before acting, so the last blow finishes first.")]
    [SerializeField, Min(0f)] private float _startDelay = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _sidestepChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _tauntChance = 0.3f;
    [SerializeField] private int _tauntCount = 3;
    [Header("Sidestep")]
    [SerializeField] private Vector2 _sidestepAngle = new Vector2(25f, 45f);
    [Tooltip("Distance to the target kept while sidestepping, clamped into this range so the next blow still reaches.")]
    [SerializeField] private Vector2 _sidestepRadius = new Vector2(2.5f, 3.3f);
    [SerializeField] private float _sidestepSpeed = 1.1f;
    [SerializeField] private float _turnSpeed = 360f;
    [Tooltip("Animator state the taunt is cut back to when the wait ends early.")]
    [SerializeField] private string _locomotionState = "Movement 0";

    private Transform _transform;
    private PerceptionInput _perception;
    private MoveInput _moveInput;
    private NavMeshAgent _agent;
    private Animator _animator;
    private Speed _speed;
    private float _timer;
    private bool _acted;
    private bool _sidestepping;
    private bool _taunting;
    private bool _agentRotated;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;
        _agent = actor.GetComponent<NavMeshAgent>();

        if (actor.TryGetInput(out PerceptionInput perception))
            _perception = perception;

        if (actor.TryGetInput(out MoveInput moveInput))
            _moveInput = moveInput;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetProperty(out Speed speed))
            _speed = speed;
    }

    public override void Enter()
    {
        base.Enter();
        _timer = _startDelay;
        _acted = false;
    }

    public override void Exit()
    {
        if (_sidestepping)
        {
            _moveInput.Stop();
            _speed?.ResetValue();
            if (_agent != null)
                _agent.updateRotation = _agentRotated;
            _sidestepping = false;
        }

        if (_taunting)
        {
            // The attack comes next: don't make it wait for the taunt to finish.
            _animator.ResetTrigger(TauntTrigger);
            if (_animator.HasState(_locomotionState))
                _animator.CrossFade(_locomotionState, 0.2f);
            _taunting = false;
        }

        base.Exit();
    }

    private void Update()
    {
        if (_perception == null || _perception.Target == null)
            return;

        FaceTarget();

        if (_acted)
            return;

        _timer -= Time.deltaTime;
        if (_timer > 0f)
            return;

        _acted = true;

        float roll = Random.value;
        if (roll < _sidestepChance && TrySidestep())
            return;

        if (roll < _sidestepChance + _tauntChance)
            Taunt();
    }

    private void FaceTarget()
    {
        Vector3 toTarget = _perception.Target.RootPoint.position - _transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f)
            return;

        _transform.rotation = Quaternion.RotateTowards(_transform.rotation,
            Quaternion.LookRotation(toTarget), _turnSpeed * Time.deltaTime);
    }

    private bool TrySidestep()
    {
        if (_agent == null || _moveInput == null || !_agent.isOnNavMesh)
            return false;

        Vector3 center = _perception.Target.RootPoint.position;
        Vector3 offset = _transform.position - center;
        offset.y = 0f;

        float radius = Mathf.Clamp(offset.magnitude, _sidestepRadius.x, _sidestepRadius.y);
        float side = Random.value < 0.5f ? -1f : 1f;

        // Try one side, then the other.
        for (int i = 0; i < 2; i++, side = -side)
        {
            float angle = side * Random.Range(_sidestepAngle.x, _sidestepAngle.y);
            Vector3 point = center + Quaternion.Euler(0f, angle, 0f) * offset.normalized * radius;

            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, _agent.areaMask))
                continue;

            var path = new NavMeshPath();
            if (!_agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                continue;

            _agentRotated = _agent.updateRotation;
            _agent.updateRotation = false;
            _speed?.SetValue(_sidestepSpeed);
            _moveInput.MoveToDestination(hit.position);
            _sidestepping = true;
            return true;
        }

        return false;
    }

    private void Taunt()
    {
        if (_animator == null || _tauntCount <= 0)
            return;

        _animator.SetInteger(TauntTypeParam, Random.Range(0, _tauntCount));
        _animator.SetTrigger(TauntTrigger);
        _taunting = true;
    }
}
