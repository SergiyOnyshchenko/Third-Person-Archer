using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Actor;

public class ElementalFeedbackPlayer : MonoBehaviour, IActorIniter
{
    [System.Serializable]
    public class ElementalFeedback
    {
        [SerializeField] private ElementalType _type;
        [SerializeField] private MMF_Player _player;
        public ElementalType Type { get => _type; }

        public void Play()
        {
            _player.PlayFeedbacks();
        }
    }

    [SerializeField] private ElementalFeedback[] _views;
    [SerializeField] private ElementalFeedback _currentView;
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

    public void Play()
    {
        if (_currentView == null)
            return;

        _currentView.Play();
    }

    private void SetCurrentView()
    {
        SetCurrentView(_elementalAttackType.Value);
    }

    public void SetCurrentView(ElementalType type)
    {
        ElementalFeedback view = GetViewByType(type);
        _currentView = view;
    }

    private ElementalFeedback GetViewByType(ElementalType type)
    {
        foreach (var view in _views)
        {
            if (view.Type == type)
                return view;
        }

        return null;
    }
}
