using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unparanter : MonoBehaviour
{
    [SerializeField] private bool _onAwake;

    private void Start()
    {
        if (_onAwake)
            Unparent();
    }

    public void Unparent()
    {
        transform.SetParent(null);
    }
} 
