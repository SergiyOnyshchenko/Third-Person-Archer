using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayInstance : MonoBehaviour
{
    [SerializeField] private GameObject _view;

    private void Start()
    {
        Show(false);
    }

    public void Show(bool value)
    {
        _view.SetActive(value);
    }
}
