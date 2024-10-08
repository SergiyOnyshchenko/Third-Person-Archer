using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Actor;
using Actor.Properties;

public class ElementalSkillCounterView : MonoBehaviour
{
    [SerializeField] private ElementalData _data;
    [SerializeField] private TextMeshProUGUI _countText;
    public ElementalData Data { get => _data; }

    private void OnEnable()
    {
        SetCountView();
        _data.OnArrowCountModifyed.AddListener(SetCountView);
    }

    private void OnDisable()
    {
        _data.OnArrowCountModifyed.RemoveListener(SetCountView);
    }

    public void TrySetElementalAttack()
    {
        _data.SendActivationEvent();
    }

    private void SetCountView()
    {
        _countText.text = _data.ArrowCount.ToString();
    }
}
