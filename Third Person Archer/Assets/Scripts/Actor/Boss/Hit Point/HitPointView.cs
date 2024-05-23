using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HitPointView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private HitPoint _hitPoint;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _animator.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (!_animator.gameObject.activeSelf)
            return;

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
        _animator.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _animator.gameObject.SetActive(false);
    }

    public void PlayDamageAnimation()
    {
        _animator.SetTrigger("Damage");
    }
}
