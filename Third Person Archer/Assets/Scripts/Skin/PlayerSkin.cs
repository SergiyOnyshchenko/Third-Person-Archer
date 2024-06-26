using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSkin
{
    [SerializeField] private int _index;
    [SerializeField] private GameObject[] _models;
    [SerializeField] private Animator _animator;
    public int Index { get => _index; }
    public GameObject[] Models { get => _models; }
    public Animator Animator { get => _animator; }

    public void ShowView(bool value)
    {
        foreach (var model in _models)
            model.SetActive(value);
    }
}
