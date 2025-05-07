using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UpgradeView : MonoBehaviour
{
    [SerializeField] private UpgradeData _data;
    [Space]
    [SerializeField] private UpgradePurchaseButton _button;
    [SerializeField] private Slider _progressSlider;
    public UnityEvent OnUpgraded;

    private void OnEnable()
    {
        _data.OnUpgraded.AddListener(UpdateView);
        _button.onPurchaseSuccess.AddListener(Upgrade);
    }

    private void OnDisable()
    {
        _data.OnUpgraded.RemoveListener(UpdateView);
        _button.onPurchaseSuccess.RemoveListener(Upgrade);
    }

    private void Start()
    {
        UpdateView();
    }

    private void Upgrade()
    {
        _data.Upgrade();
        UpdateView();
    }

    private void UpdateView()
    {
        if (_data.CanBeUpgraded)
            _button.SetPrice(_data.UpgradeCost);

        _button.gameObject.SetActive(_data.CanBeUpgraded);

        _progressSlider.DOValue(_data.UpgradeRatio, 0.5f).SetDelay(0.5f);
    }
}
