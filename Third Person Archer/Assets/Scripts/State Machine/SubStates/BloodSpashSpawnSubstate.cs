using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class BloodSpashSpawnSubstate : SubState, IActorIniter
{
    [SerializeField] private FakeDecal _bloodPrefab;
    [SerializeField] private int _maxSpawnCount = 3;
    [SerializeField] private float _spawnDelay = 0.25f;

    private Hitbox[] _hitboxes;
    private List<FakeDecal> _bloodSplashes = new List<FakeDecal>();
    private float _spawnDelayTimer = 0;

    public bool CanSpawn { get => _spawnDelayTimer <= 0; }

    public void InitActor(ActorController actor)
    {
        _hitboxes = actor.GetComponentsInChildren<Hitbox>();
    }

    public override void Enter()
    {
        base.Enter();

        SubscribeToCollisions();
    }

    public override void Exit()
    {
        UnsubscribeFromCollisions();

        base.Exit();
    }

    private void Update()
    {
        if(_spawnDelayTimer > 0)
            _spawnDelayTimer -= Time.deltaTime;
    }

    private void SubscribeToCollisions()
    {
        foreach (var hitbox in _hitboxes)
            hitbox.OnCollided.AddListener(TrySpawnBlood);
    }

    private void UnsubscribeFromCollisions()
    {
        foreach (var hitbox in _hitboxes)
            hitbox.OnCollided.RemoveListener(TrySpawnBlood);
    }

    private void TrySpawnBlood(Collision collision)
    {
        if (CanSpawn && _bloodSplashes.Count < _maxSpawnCount)
        {
            _spawnDelayTimer = _spawnDelay;

            FakeDecal blood = Instantiate(_bloodPrefab, collision.contacts[0].point, Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal));
            _bloodSplashes.Add(blood);
        }
    }
}
