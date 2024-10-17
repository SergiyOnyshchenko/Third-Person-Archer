using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowElementalCounterView : MonoBehaviour
{
    private ElementalSkillCounterView[] _views;

    private void Awake()
    {
        _views = GetComponentsInChildren<ElementalSkillCounterView>();
    }

    private void OnEnable()
    {
        foreach (var view in _views)
        {
            view.Data.OnArrowCountModifyed.AddListener(UpdateView);
        }

        UpdateView();
    }

    private void OnDisable()
    {
        foreach (var view in _views)
        {
            view.Data.OnArrowCountModifyed.RemoveListener(UpdateView);
        }
    }

    private void UpdateView()
    {
        foreach (var view in _views)
        {
            if (view.Data.ArrowCount > 0)
            {
                view.gameObject.SetActive(true);
            }
            else
            {
                view.gameObject.SetActive(false);
            }
        }
    }
}
