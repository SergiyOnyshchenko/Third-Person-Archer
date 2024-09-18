using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour, IActorIniter
{
    [SerializeField] private Slider _bar;
    private Mana _mana;
    private float _updateDuration = 0.5f;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out Mana mana))
        {
            _mana = mana;
            _mana.OnManaModified.AddListener(ModifyMana);

            ModifyMana(_mana.Ratio);
        }
    }

    private void OnEnable()
    {
        if (_mana != null)
            _mana.OnManaModified.AddListener(ModifyMana);
    }

    private void OnDisable()
    {
        if (_mana != null)
            _mana.OnManaModified.RemoveListener(ModifyMana);
    }

    private void ModifyMana(float ratio)
    {
        _bar.DOValue(ratio, _updateDuration);
    }
}
