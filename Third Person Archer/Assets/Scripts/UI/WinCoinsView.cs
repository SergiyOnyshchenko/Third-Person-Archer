using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WinCoinsView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private float _updateDuration = 1f;
    private int _currentValue = 0;
    private int _targetValue = 0;

    private void Awake()
    {
        _textField.text = "+" + _currentValue.ToString();
    }

    private void OnEnable()
    {
        UpdateView();
    }

    public void Init(int target)
    {
        _targetValue = target;
        UpdateView();
    }

    private void UpdateView()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_currentValue == _targetValue)
            return;

        DOVirtual.Int(_currentValue, _targetValue, _updateDuration, (int value) =>
        {
            SetText(value);
        });
    }

    private void SetText(int value)
    {
        _currentValue = value;
        _textField.text = "+" + value.ToString();
    }
}
