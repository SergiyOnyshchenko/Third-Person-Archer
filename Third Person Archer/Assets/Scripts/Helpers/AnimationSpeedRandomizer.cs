using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationSpeedRandomizer : MonoBehaviour
{
    [SerializeField] private string _propertyName = "SpeedRandomizer";
    [SerializeField] private bool _randomizwOverTime;
    [SerializeField] private float _randomizeDelay = 1;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        RandomizeAnimation();

        StartCoroutine(RandomizeOverTime(_randomizeDelay));
    }

    public void RandomizeAnimation()
    {
        float randomizedValue = Random.Range(0.75f, 1.25f);
        _animator.SetFloat(_propertyName, randomizedValue);
    }

    private IEnumerator RandomizeOverTime(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            RandomizeAnimation();
        }
    }
}
