using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

public class SimpleElementalView : MonoBehaviour
{
    [SerializeField] private ElementalViewEvents[] _views;
    private ElementalViewEvents _currentView;

    public void SetCurrentView(ElementalType type)
    {
        ElementalViewEvents view = GetViewByType(type);

        if (view == null)
            return;

        if(_currentView == view)
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
