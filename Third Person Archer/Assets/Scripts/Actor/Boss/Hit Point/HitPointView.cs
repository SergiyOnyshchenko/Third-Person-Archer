using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HitPointView : MonoBehaviour
{
    private Animator _animator;
    private HitPoint _hitPoint;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _animator = GetComponent<Animator>();

        StopAnimation();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        Vector3 screenPos = _camera.WorldToScreenPoint(_hitPoint.transform.position);
        transform.position = screenPos;
    }

    public void Init(HitPoint hitPoint)
    {
        _hitPoint = hitPoint;

        _hitPoint.OnActivated.AddListener(Activate);
        _hitPoint.OnDeactivated.AddListener(Deactivate);
        _hitPoint.OnDamaged.AddListener(PlayDamageAnimation);
    }

    public void Activate()
    {
        PlayIdleAnimation();
    }

    public void Deactivate()
    {
        StopAnimation();
    }

    public void PlayIdleAnimation()
    {
        _animator.SetTrigger("Show");
    }

    public void PlayDamageAnimation()
    {
        _animator.SetTrigger("Damage");
    }

    public void StopAnimation()
    {
        _animator.SetTrigger("Hide");
    }
}
