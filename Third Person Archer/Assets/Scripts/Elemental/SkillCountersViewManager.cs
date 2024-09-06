using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using Actor;
using UnityEngine;
using UnityEngine.Events;

public class SkillCountersViewManager : MonoBehaviour
{
    [SerializeField] private ElementalSkillCounterView _fireSkillCounter;
    [SerializeField] private ElementalSkillCounterView _frostSkillCounter;
    private ElementalSkillCounterView[] _allSkillCounters;
    private ElementalArrowsCount _elementalArrowsCount;
    private ElementalAttackType _elementalAttackType;

    private void Start()
    {
        _allSkillCounters = new ElementalSkillCounterView[] { _fireSkillCounter, _frostSkillCounter };

        foreach (var counter in _allSkillCounters)
            counter.gameObject.SetActive(false);
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out ElementalAttackType elementalAttackType))
            _elementalAttackType = elementalAttackType;

        if (actor.TryGetProperty(out ElementalArrowsCount elementalArrowsCount))
            _elementalArrowsCount = elementalArrowsCount;
    }


}
