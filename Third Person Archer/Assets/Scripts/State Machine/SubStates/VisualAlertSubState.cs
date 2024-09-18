using DG.Tweening;
using UnityEngine;

public class VisualAlertSubState : SubState
{
    [SerializeField] private Sprite _icon;

    private Transform _alertMessageTransform;
    private Camera _cameraReference;
    private Tween _appearanceTween;
    public override void Enter()
    {
        base.Enter();

        _cameraReference = Camera.main;
        CreateAlertMessage();
    }

    private void Update()
    {
        if (_alertMessageTransform != null && _cameraReference != null)
        {
            _alertMessageTransform.position = transform.position + Vector3.up * 1.9f + _cameraReference.transform.right * .4f;
            _alertMessageTransform.rotation = _cameraReference.transform.rotation;
        }
    }

    public override void Exit()
    {
        _appearanceTween?.Kill();
        Destroy(_alertMessageTransform.gameObject);

        base.Exit();
    }

    private void CreateAlertMessage()
    {
        GameObject go = new GameObject();
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _icon;
        _alertMessageTransform = go.transform;

        _alertMessageTransform.transform.localScale = Vector3.zero;

        _appearanceTween = _alertMessageTransform.DOScale(1, 0.25f).SetEase(Ease.OutBack);

    }
}
