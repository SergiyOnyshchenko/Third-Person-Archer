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
    public Slider Slider { get => _bar; }
    public Health Health { get => _health; }

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

    /// <summary>A short bump of the bar when a soul flies into it.</summary>
    public void PlayHealPulse()
    {
        _bar.transform.DOKill(true);
        _bar.transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 6, 0.5f);
    }
}
