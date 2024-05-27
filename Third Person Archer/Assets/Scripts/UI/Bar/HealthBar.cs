using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBar : MonoBehaviour, IActorIniter
{
    [SerializeField] private Slider _bar;
    private Health _health;
    private float _updateDuration = 0.5f;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out Health health))
            _health = health;

        _health.OnHealthModified.AddListener(ModifyHealth);
    }

    private void OnEnable()
    {
        if(_health != null)
            _health.OnHealthModified.AddListener(ModifyHealth);
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnHealthModified.RemoveListener(ModifyHealth);
    }

    private void ModifyHealth(int health, int maxHealth)
    {
        _bar.DOValue((float)health/maxHealth, _updateDuration);
    }
}
