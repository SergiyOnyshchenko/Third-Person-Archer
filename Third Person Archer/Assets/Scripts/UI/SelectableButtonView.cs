using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButtonView : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [Space]
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _deselextedSprite;
    private ISelectable _selectable;

    private void Awake()
    {
        _selectable = GetComponent<ISelectable>();
    }

    private void OnEnable()
    {
        _selectable.OnSelected += Select;
        _selectable.OnDeselected += Deselect;
    }

    private void OnDisable()
    {
        _selectable.OnSelected -= Select;
        _selectable.OnDeselected -= Deselect;


    }

    private void Select()
    {
        _backgroundImage.sprite = _selectedSprite;
    }

    private void Deselect()
    {
        _backgroundImage.sprite = _deselextedSprite;
    }
}
