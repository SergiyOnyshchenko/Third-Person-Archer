using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class ElementalView : MonoBehaviour, IActorIniter
{
    [SerializeField] private ElementalViewEvents[] _views;
    private ElementalViewEvents _currentView;
    private ElementalProperty _elemental;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _elemental)) 
        {
            SetCurrentView();
        }

        if (_elemental != null)
            _elemental.OnPropertyChanged += SetCurrentView;
    }

    private void OnDestroy()
    {
        if (_elemental != null)
            _elemental.OnPropertyChanged -= SetCurrentView;
    }

    public void SetCurrentView()
    {
        SetCurrentView(_elemental.Value);
    }

    public void SetCurrentView(ElementalType type)
    {
        ElementalViewEvents view = GetViewByType(type);

        if (view == null)
            return;

        if (_currentView == view)
            return;

        if (_currentView != null)
            _currentView.Reset();

        _currentView = view;
        _currentView.Apply();
    }

    private ElementalViewEvents GetViewByType(ElementalType type)
    {
        foreach (var view in _views)
        {
            if (view.Type == type)
                return view;
        }

        return null;
    }
}
