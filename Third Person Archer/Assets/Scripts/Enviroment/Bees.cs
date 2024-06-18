using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;

public class Bees : MonoBehaviour
{
    [SerializeField] private float _speed = 10;
    [SerializeField] private LayerMask _enemiesMask;
    private ActorController _targetEnemy;
    private ITarget _target;
    private bool _isAttacked;
    public UnityEvent OnAttacked = new UnityEvent();

    private void Start()
    {
        StartCoroutine(Activate());
    }

    private void Update()
    {
        if (_target == null)
            return;

        if (Vector3.Distance(_target.TargetPoint.position, transform.position) <= 0.1f)
        {
            if (!_isAttacked)
                Attack();
        }
        else
        {
            Vector3 direction = _target.TargetPoint.position - transform.position;
            transform.position = transform.position + direction.normalized * _speed * Time.deltaTime;
        }
    }

    public IEnumerator Activate()
    {
        yield return new WaitForSeconds(1);

        if (TryFindNearestEnemy(out ActorController enemy))
        {
            _targetEnemy = enemy;

            if (enemy.TryGetSystem(out Target target))
                _target = target;
        }
    }

    private void Attack()
    {
        _isAttacked = true;

        if (_targetEnemy.TryGetInput(out ActionInput actionInput))
        {
            actionInput.TryDoAction("Sting");
            OnAttacked?.Invoke();
        }
    }

    private bool TryFindNearestEnemy(out ActorController enemy)
    {
        enemy = null; 
        List<ActorController> enemies = new List<ActorController> ();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 25f);

        foreach (var hitCollider in hitColliders)
        { 
            if (hitCollider.TryGetComponent(out ActorController actor) && actor.TryGetSystem(out Health health))
            {
                if (health.IsDead)
                    continue;

                enemies.Add(actor);
            }
        }

        if(enemies.Count == 0)
            return false;

        enemy = enemies[0];

        foreach (var nearestEnemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) > 
                Vector3.Distance(transform.position, nearestEnemy.transform.position))
            {
                enemy = nearestEnemy;
            }
        }

        return true;
    }
}
