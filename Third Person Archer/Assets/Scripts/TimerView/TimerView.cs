using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerView : MonoBehaviour
{
    [SerializeField] private Slider _bar;
    [SerializeField] private GameObject _parent;

    public void Show()
    {
        _parent.SetActive(true);
    }

    public void Hide()
    {
        _parent.SetActive(false);
    }

    public void UpdateBar(float ratio)
    {
        _bar.value = ratio;
    }
}
