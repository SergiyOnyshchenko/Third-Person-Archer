using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Actor;
using Actor.Properties;

public class Projectile : MonoBehaviour, IActorIniter
{
    [field: SerializeField] public ActorController Actor { get; private set; }

    //Properties
    [SerializeField] private ProjectileHitLayermask _hitLayermask;
    private ProjectileMoveTypeProperty _moveType;
    private ProjectileDirection _direction;
    private ElementalProperty _elemental;
    private Speed _speed;
    private Gravity _gravity;
    private Damage _damage;
    private Range _range;
    private TraveledDistance _traveledDistance;

    //Systems
    private FeedbackManager _feedbackManager;
    private ProjectileHitHandler _hitHandler;
    private ProjectileHitPredictor _hitPredictor;

    private int _damageValue;

    public float TraveledDistance => _traveledDistance.Value;
    public LayerMask HitLayers { get => _hitLayermask.Value; }
    public int Damage { get => _damage.Value; }
    public Speed Speed { get => _speed; }
    public bool IsShooted { get; private set; }

    public UnityEvent OnShooted = new UnityEvent();
    public UnityEvent OnHited = new UnityEvent();
    public UnityEvent OnTargetHited = new UnityEvent();


    private void Awake()
    {
        Actor = GetComponent<ActorController>();
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _moveType)) { }
        if (actor.TryGetProperty(out _direction)) { }
        if (actor.TryGetProperty(out _elemental)) { }
        if (actor.TryGetProperty(out _speed)) { }
        if (actor.TryGetProperty(out _range)) { }
        if (actor.TryGetProperty(out _gravity)) { }

        if (actor.TryGetProperty(out _damage))
            _damage.SetValue(_damageValue);

        if (actor.TryGetProperty(out _traveledDistance)) { }

        if (actor.TryGetSystem(out _feedbackManager)) { }
        if (actor.TryGetSystem(out _hitHandler)) { }
        if (actor.TryGetSystem(out _hitPredictor)) { }
    }

    public void Shoot(Vector3 direction, float power, UnityAction onHited)
    {
        _direction.SetValue(direction);

        IsShooted = true;
        OnShooted?.Invoke();

        _hitHandler.OnTargetHited.AddListener(onHited);
        _hitHandler.OnHited = OnHited;

        //PreCheckTargetDeath();
    }

    public void SetMoveType(ProjectileMoveType type)
    {
        if (_moveType == null)
            return;

        _moveType.SetValue(type);
    }

    public void SetDamage(int damage)
    {
        _damageValue = damage;
        if (_damage != null)
            _damage.SetValue(_damageValue);
    }

    public void SetElementalType(ElementalType type)
    {
        _elemental.SetValue(type);
    }

    public void SetSpeed(float speed)
    {
        if (_speed == null) return;
        _speed.SetValue(speed);
    }

    public void SetRange(float range)
    {
        if (_range == null) return;
        _range.SetValue(range);
    }

    public void SetGravity(float gravity)
    {
        if (_gravity == null) return;
        _gravity.SetValue(gravity);
    }

    public void EnableFeedbacks(bool value)
    {
        if (_feedbackManager == null)
            return;

        _feedbackManager.SetAllFeedbacksEnabled(value);
    }

    public RaycastHit GetPredictiveHit()
    {
        if(_hitPredictor == null)
            return new RaycastHit();
        
        return _hitPredictor.GetPredictiveHit();
    }

    public bool PreCheckTargetDeath()
    {
        if(_hitPredictor == null)
            return false;

        return _hitPredictor.PreCheckTargetDeath();
    }
}
