using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SplineMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Spline _spline;
    [SerializeField] private Ease _ease;
    public UnityEvent OnMovingFinished = new UnityEvent();

    public void Move(float duration)
    {
        float value = 0f;

        DOTween.To(() => value, x => value = x, 1f, duration)
        .OnUpdate(() =>
        {
            _target.transform.position = _spline.CalculatePosition(value);
        }).
        SetEase(_ease).
        OnComplete(FinishMoving);
    }

    private void FinishMoving()
    {
        OnMovingFinished?.Invoke();
    }
}
