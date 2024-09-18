using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestroyedPart : MonoBehaviour
{
    private void Awake()
    {
        transform.DOScale(0f, 1f).SetDelay(1).OnComplete(() => gameObject.SetActive(false));
    }
}
