using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Actor;
using Actor.Properties;

public class ElementalSkillCounterView : MonoBehaviour, IActorIniter
{
    [SerializeField] private ElementalType _type;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private GameObject _view;
    private ElementalArrowsCount _elementalArrowsCount;
    private ElementalAttackType _elementalAttackType;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
            _elementalAttackType = elementalAttackType;

        if (actor.TryGetProperty(out ElementalArrowsCount elementalArrowsCount))
            _elementalArrowsCount = elementalArrowsCount;

        SetCountView();
        _elementalArrowsCount.OnPropertyChanged += SetCountView;
    }

    private void OnEnable()
    {
        if (_elementalAttackType == null || _elementalAttackType == null)
            return;

        SetCountView();
        _elementalArrowsCount.OnPropertyChanged += SetCountView;
    }

    private void OnDisable()
    {
        if (_elementalAttackType == null || _elementalAttackType == null)
            return;

        _elementalArrowsCount.OnPropertyChanged -= SetCountView;
    }

    private void SetCountView()
    {
        if (_elementalArrowsCount.Value > 0 && _elementalAttackType.Value == _type)
        {
            _view.SetActive(true);
        }
        else
        {
            _view.SetActive(false);
        }

        _countText.text = _elementalArrowsCount.Value.ToString();
    }
}
