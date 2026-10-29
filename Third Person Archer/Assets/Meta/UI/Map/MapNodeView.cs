using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MapNodeState
{
    Completed,
    Current,
    Locked,
}

/// <summary>One Campaign level on the zone map: medallion, number, class of the level, state effects.</summary>
public sealed class MapNodeView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _medallion;
    [SerializeField] private TMP_Text _number;
    [SerializeField] private Image _classIcon;
    [SerializeField] private GameObject _check;
    [SerializeField] private GameObject _lock;

    [Header("Current level effects")]
    [SerializeField] private Image _glow;
    [SerializeField] private Image _ring;
    [SerializeField] private Image _statusDot;

    [Header("Sprites")]
    [SerializeField] private Sprite _completedSprite;
    [SerializeField] private Sprite _currentSprite;
    [SerializeField] private Sprite _lockedSprite;

    [Header("Heroic mode sprites (fallback: normal)")]
    [SerializeField] private Sprite _heroicCompletedSprite;
    [SerializeField] private Sprite _heroicCurrentSprite;
    [SerializeField] private Color _heroicGlowColor = new Color(1f, 0.35f, 0.25f, 0.9f);

    private Color _glowColor;
    private bool _glowColorCached;

    public event Action Clicked;

    private DG.Tweening.Sequence _pulse;
    private MapNodeState _state;

    private void Awake()
    {
        if (_button != null) _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        KillPulse();
    }

    private void OnEnable()
    {
        if (_state == MapNodeState.Current) StartPulse();
    }

    public void Bind(MapNodeState state, int number, Sprite classIcon, Color? statusColor, bool heroic = false)
    {
        _state = state;

        if (_medallion != null)
            _medallion.sprite = state switch
            {
                MapNodeState.Completed => heroic && _heroicCompletedSprite != null ? _heroicCompletedSprite : _completedSprite,
                MapNodeState.Current => heroic && _heroicCurrentSprite != null ? _heroicCurrentSprite : _currentSprite,
                _ => _lockedSprite
            };

        if (_glow != null)
        {
            if (!_glowColorCached) { _glowColor = _glow.color; _glowColorCached = true; }
            _glow.color = heroic ? _heroicGlowColor : _glowColor;
        }

        if (_number != null)
        {
            _number.text = number.ToString();
            _number.gameObject.SetActive(state != MapNodeState.Current);
            _number.color = state == MapNodeState.Locked ? new Color(0.75f, 0.73f, 0.7f) : Color.white;
        }

        if (_classIcon != null)
        {
            _classIcon.sprite = classIcon;
            _classIcon.gameObject.SetActive(state == MapNodeState.Current && classIcon != null);
        }

        if (_check != null) _check.SetActive(state == MapNodeState.Completed);
        if (_lock != null) _lock.SetActive(state == MapNodeState.Locked);

        bool current = state == MapNodeState.Current;
        if (_glow != null) _glow.gameObject.SetActive(current);
        if (_ring != null) _ring.gameObject.SetActive(current);

        if (_statusDot != null)
        {
            _statusDot.gameObject.SetActive(current && statusColor.HasValue);
            if (statusColor.HasValue) _statusDot.color = statusColor.Value;
        }

        transform.localScale = current ? Vector3.one * 1.15f : state == MapNodeState.Locked ? Vector3.one * 0.85f : Vector3.one;

        if (current && isActiveAndEnabled) StartPulse();
        else KillPulse();
    }

    private void StartPulse()
    {
        KillPulse();
        if (_glow == null || _ring == null) return;

        _glow.transform.localScale = Vector3.one;
        _ring.transform.localScale = Vector3.one * 0.8f;
        var ringColor = _ring.color; ringColor.a = 1f; _ring.color = ringColor;

        _pulse = DOTween.Sequence().SetLink(gameObject)
            .Append(_glow.transform.DOScale(1.18f, 0.9f).SetEase(Ease.InOutSine))
            .Join(_ring.transform.DOScale(1.6f, 1.8f).SetEase(Ease.OutQuad))
            .Join(_ring.DOFade(0f, 1.8f).SetEase(Ease.InQuad))
            .Insert(0.9f, _glow.transform.DOScale(1f, 0.9f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);
    }

    private void KillPulse()
    {
        _pulse?.Kill();
        _pulse = null;
    }

    private void OnClick()
    {
        transform.DOKill();
        float baseScale = _state == MapNodeState.Current ? 1.15f : _state == MapNodeState.Locked ? 0.85f : 1f;
        transform.localScale = Vector3.one * baseScale;
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 8, 0.6f).SetLink(gameObject);
        Clicked?.Invoke();
    }
}
