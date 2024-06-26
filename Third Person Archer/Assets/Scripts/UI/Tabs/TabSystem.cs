using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabSystem : MonoBehaviour
{
    [SerializeField] private Tab[] _tabs;

    private void OnEnable()
    {
        foreach (var tab in _tabs)
        {
            tab.Subscribe();
            tab.OnSelected += OpenTab;
        }
    }

    private void OnDisable()
    {
        foreach (var tab in _tabs)
        {
            tab.OnSelected -= OpenTab;
            tab.Unsubscribe();
        }   
    }

    private void Start()
    {
        OpenTab(_tabs[0]);
    }

    private void OpenTab(Tab target)
    {
        foreach (var tab in _tabs)
        {
            if (tab != target)
                tab.Close();
        }

        target.Open();
    }
}
