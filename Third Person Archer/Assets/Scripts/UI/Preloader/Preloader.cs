using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class Preloader : MonoBehaviour
{
    [SerializeField] private Image _fade;
    private const float _duration = 1f;
    public static Preloader Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _fade.gameObject.SetActive(true);
        FadeOut();
    }

    public void FadeIn(UnityAction onFinish)
    {
        _fade.gameObject.SetActive(true);

        Color fadeColor = _fade.color;
        fadeColor.a = 1;
        _fade.DOColor(fadeColor, _duration).OnComplete(() => onFinish?.Invoke());
    }

    public void FadeOut()
    {
        Color fadeColor = _fade.color;
        fadeColor.a = 0;
        _fade.DOColor(fadeColor, _duration).OnComplete(() => _fade.gameObject.SetActive(false));
    }
}
