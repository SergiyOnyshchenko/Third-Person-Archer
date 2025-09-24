using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Actor;
using Actor.Properties;

public class Projectile : MonoBehaviour, IActorIniter
{
    [field: SerializeField] public ActorController Actor {  get; private set; }

    //Properties
    [SerializeField] private ProjectileHitLayermask _hitLayermask;
    private ProjectileDirection _direction;
    private ElementalProperty _elemental;
    private Speed _speed;
    private Damage _damage;
    private Actor.Properties.Range _range;
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

    public UnityEvent OnShooted = new UnityEvent();
    public UnityEvent OnHited = new UnityEvent();
    public UnityEvent OnTargetHited = new UnityEvent();


    private void Awake()
    {
        Actor = GetComponent<ActorController>();
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _direction)) { }
        if (actor.TryGetProperty(out _elemental)) { }
        if (actor.TryGetProperty(out _speed)) { }
        if (actor.TryGetProperty(out _range)) { }

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

        OnShooted?.Invoke();
        _hitHandler.OnTargetHited.AddListener(onHited);

        _hitHandler.OnHited = OnHited;

        PreCheckTargetDeath();
    }

    public void SetDamage(int damage)
    {
        Debug.Log("DEMAGE SETTED " + damage);

        _damageValue = damage;

        if(_damage != null)
            _damage.SetValue(_damageValue);
    }

    public void SetElementalType(ElementalType type)
    {
        _elemental.SetValue(type);
    }

    public void SetSpeed(float newSpeed)
    {
        if (_speed == null)
            return;

        _speed.SetValue(newSpeed);
    }

    public void SetRange(float range)
    {
        //if (_range == null)
        //    return;

        //_range.SetValue(range);
    }

    public void EnableFeedbacks(bool value)
    {
        if (_feedbackManager == null)
            return;

        _feedbackManager.SetAllFeedbacksEnabled(value);
    }

    public RaycastHit GetPredictiveHit()
    {
        return _hitPredictor.GetPredictiveHit();
    }

    public bool PreCheckTargetDeath()
    {
        return _hitPredictor.PreCheckTargetDeath();
    }
}
