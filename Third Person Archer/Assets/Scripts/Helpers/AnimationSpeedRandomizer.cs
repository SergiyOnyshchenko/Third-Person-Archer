using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedRandomizer : MonoBehaviour
{
    [SerializeField] private string _propertyName = "SpeedRandomizer";

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        RandomizeAnimation();
    }

    public void RandomizeAnimation()
    {
        float randomizedValue = Random.Range(0.75f, 1.25f);
        _animator.SetFloat(_propertyName, randomizedValue);
    }
}
