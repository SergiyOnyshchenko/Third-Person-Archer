using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Heroic mode on the map: zone arrows (every zone is open after the campaign: Heroic levels,
/// unfinished Sniper, boss replays) and a "HEROIC n/m" badge for the shown zone.
/// Before Heroic the arrows stay hidden (only the current zone is playable).
/// </summary>
public sealed class ZoneBrowseView : MonoBehaviour
{
    [SerializeField] private Button _left;
    [SerializeField] private Button _right;
    [SerializeField] private GameObject _heroicBadge;
    [SerializeField] private TMP_Text _heroicText;
    [SerializeField] private RectTransform _mapsRoot;
    [SerializeField] private MissionPanelView _panel;

    private MainMenuServices _services;

    private void Awake()
    {
        _left?.onClick.AddListener(() => Browse(-1));
        _right?.onClick.AddListener(() => Browse(+1));
    }

    private void OnEnable()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            Init(runtime.Services);
            return;
        }

        MainMenuRuntime.Ready -= Init;
        MainMenuRuntime.Ready += Init;
    }

    private void OnDisable()
    {
        MainMenuRuntime.Ready -= Init;
    }

    private void OnDestroy()
    {
        if (_services != null) _services.OnMenuStateChanged -= Refresh;
    }

    private void Init(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= Init;
        if (_services == null)
        {
            _services = services;
            _services.OnMenuStateChanged += Refresh;
        }

        Refresh();
    }

    private void Refresh()
    {
        var progress = _services?.ProgressData;
        if (progress == null) return;

        bool heroic = progress.IsHeroicUnlocked();
        int zoneIndex = progress.ZoneIndex;
        int last = progress.GetLastUnlockedZoneIndex();

        SetArrow(_left, heroic, zoneIndex > 0);
        SetArrow(_right, heroic, zoneIndex < last);

        if (_heroicBadge != null) _heroicBadge.SetActive(heroic);
        if (heroic && _heroicText != null)
        {
            var zone = progress.Zone;
            int count = progress.GetHeroicLevelCount(zone);
            int done = progress.GetHeroicCompleted(zone);

            _heroicText.text = done >= count
                ? $"HEROIC <color=#FFD861>CLEARED</color> · run {done / Mathf.Max(1, count) + 1}"
                : $"HEROIC {done}/{count}";
        }
    }

    private static void SetArrow(Button arrow, bool visible, bool enabled)
    {
        if (arrow == null) return;
        arrow.gameObject.SetActive(visible);
        arrow.interactable = enabled;

        var image = arrow.targetGraphic;
        if (image != null) image.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    private void Browse(int step)
    {
        var progress = _services?.ProgressData;
        if (progress == null) return;

        int target = Mathf.Clamp(progress.ZoneIndex + step, 0, progress.GetLastUnlockedZoneIndex());
        if (target == progress.ZoneIndex) return;

        if (_panel != null) _panel.Hide();
        _services.Progress.SelectZone(target);

        // Campaign first: the Heroic level of the new zone is the next step there.
        _services.Progress.SelectMissionType(MissionType.Campaign);

        if (_mapsRoot != null)
        {
            _mapsRoot.DOKill(complete: true);
            _mapsRoot.anchoredPosition += new Vector2(step * 120f, 0f);
            _mapsRoot.DOAnchorPos(_mapsRoot.anchoredPosition - new Vector2(step * 120f, 0f), 0.35f)
                .SetEase(Ease.OutCubic).SetLink(_mapsRoot.gameObject);
        }

        var arrow = step < 0 ? _left : _right;
        if (arrow != null)
        {
            arrow.transform.DOKill(complete: true);
            arrow.transform.DOPunchScale(Vector3.one * 0.18f, 0.25f, 8, 0.6f).SetLink(arrow.gameObject);
        }
    }
}
