using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ActorElementalView : ElementalView, IActorIniter
{
    private ElementalAttackType _elementalAttackType;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
        {
            _elementalAttackType = elementalAttackType;
            SetCurrentView();

            _elementalAttackType.OnPropertyChanged += SetCurrentView;
        }
    }

    private void OnEnable()
    {
        if (_elementalAttackType == null)
            return;

        SetCurrentView();
        _elementalAttackType.OnPropertyChanged += SetCurrentView;
    }

    private void OnDisable()
    {
        if (_elementalAttackType == null)
            return;

        _elementalAttackType.OnPropertyChanged -= SetCurrentView;
    }

    private void SetCurrentView()
    {
        SetCurrentView(_elementalAttackType.Value);
    }
}
