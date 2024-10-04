using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine.Events;
using UnityEngine;
using DG.Tweening;

public class ManaDrop : MonoBehaviour
{
    enum ManaState
    {
        Spawned,
        MovingToTarget,
        Finished
    }

    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _delay = 0.5f;
    private ActorController _target;
    private Transform _movePoint;
    private int _value = 10;
    private ManaState _state = ManaState.Spawned;
    public UnityEvent OnSpawned = new UnityEvent();
    public UnityEvent OnStartMoving = new UnityEvent();
    public UnityEvent OnFinishMoving = new UnityEvent();

    public void Init(ActorController target, int value)
    {
        _target = target;
        _value = value;

        if (_target.TryGetSystem(out Actor.Target moveTarget))
        {
            _movePoint = moveTarget.TargetPoint;
        }
        else
        {
            _movePoint = _target.transform;
        }

        OnSpawned?.Invoke();

        DOVirtual.DelayedCall(_delay, StartMoving);
    }

    private void Update()
    {
        if (_state == ManaState.MovingToTarget)
        {
            var step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _movePoint.position, step);
            transform.LookAt(_movePoint.position);

            if ((Vector3.Distance(transform.position, _movePoint.position) < 0.1f))
                FinishMoving();
        }
    }

    private void StartMoving()
    {
        _state = ManaState.MovingToTarget;
        OnStartMoving?.Invoke();
    }

    private void FinishMoving()
    {
        _state = ManaState.Finished;

        if (_target.TryGetProperty(out Mana mana))
            mana.Add(_value);
            
        OnFinishMoving?.Invoke();
    }
}
