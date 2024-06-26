using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinSelectorManager : MonoBehaviour
{
    [SerializeField] private EquippedPlayerSkinData _equippedData;
    private IPlayerSkinSelector[] _selectors;

    private void Awake()
    {
        _selectors = GetComponentsInChildren<IPlayerSkinSelector>(); 
    }

    private void OnEnable()
    {
        foreach (var selector in _selectors)
            selector.OnSkinSelected += Select;

        StartCoroutine(Init());
    }

    private void OnDisable()
    {
        foreach (var selector in _selectors)
            selector.OnSkinSelected -= Select;
    }
    
    private IEnumerator Init()
    {
        yield return null;

        foreach (var selector in _selectors)
        {
            if (selector.SkinData == _equippedData.SkinData)
                Select(selector);
        }
    }

    private void Select(IPlayerSkinSelector target)
    {
        foreach (var selector in _selectors)
        {
            if(target == selector)
            {
                selector.Selector.Select();
                _equippedData.Equip(target.SkinData);
            }
            else
            {
                selector.Selector.Deselect();
            }
        }
    }
}
