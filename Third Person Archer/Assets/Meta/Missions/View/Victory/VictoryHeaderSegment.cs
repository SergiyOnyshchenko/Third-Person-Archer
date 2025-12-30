using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class VictoryHeaderSegment : MonoBehaviour, IVictorySegment
{
    [SerializeField] private CanvasGroup _rootGroup;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _chapterText;

    [SerializeField] private float _fadeDuration = 0.3f;

    public event Action OnShown;

    private Tween _tween;

    public void Show(VictoryContext context, FinalRewardBundle rewards)
    {
        gameObject.SetActive(true);

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = false;
        }

        if (_titleText != null)
        {
            _titleText.text = context.missionType switch
            {
                MissionType.Boss => "Boss Defeated",
                MissionType.Contracts => "Contract Completed",
                MissionType.Sniper => "Sniper Mission Completed",
                _ => "Completed" // Campaign default
            };
        }

        // Zone {zoneIndex+1} / Campaign {indexInsideZone+1}
        if (_chapterText != null)
        {
            if (context.missionType == MissionType.Campaign)
            {
                _chapterText.gameObject.SetActive(true);
                _chapterText.text = $"Mission {context.zoneIndex + 1} - {context.indexInsideZone + 1}";
            }
            else
            {
                _chapterText.gameObject.SetActive(false);
            }
        }

        _tween?.Kill();
        if (_rootGroup != null)
        {
            _tween = _rootGroup.DOFade(1f, _fadeDuration)
                .OnComplete(() =>
                {
                    _rootGroup.interactable = true;
                    _rootGroup.blocksRaycasts = true;
                    OnShown?.Invoke();
                });
        }
        else
        {
            OnShown?.Invoke();
        }
    }

    public void Skip()
    {
        _tween?.Kill();

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 1f;
            _rootGroup.interactable = true;
            _rootGroup.blocksRaycasts = true;
        }

        OnShown?.Invoke();
    }
}