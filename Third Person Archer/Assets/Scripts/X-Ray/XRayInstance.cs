using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayInstance : MonoBehaviour, IXRayInstance
{
    [SerializeField] private GameObject _view;

    private void Start()
    {
        ActivateXRay(false);
    }

    public void ActivateXRay(bool value)
    {
        _view.SetActive(value);
    }
}
