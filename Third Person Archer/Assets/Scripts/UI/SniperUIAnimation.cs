using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UIElements;

public class SniperUIAnimation : MonoBehaviour, IActorIniter
{
    [SerializeField] private Transform _transform;
    [SerializeField] private CanvasGroup _canvasGroup;
     
    private WeaponPull _pull;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetProperty(out _pull)) { }
    }

    private void Update()
    {
        if (_pull == null)
            return;

        float scale = Mathf.Lerp(2.5f, 1f, _pull.Value);
        float transparancy = Mathf.Lerp(0f, 1f, _pull.Value);

        _transform.localScale = new Vector3(scale, scale, scale);
        _canvasGroup.alpha = transparancy;
    }
}
