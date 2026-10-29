using DG.Tweening;
using Meta.Economy;
using Meta.Weapons;
using Meta.Weapons.UI;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Parchment panel that slides in from the right when a map node or landmark is tapped:
/// weapon of the level, "your damage / R" bar with the 1–2 shot status, enemy roster
/// (body/head shots), reward, and Play / Upgrade / Guild Board actions.
/// </summary>
public sealed class MissionPanelView : MonoBehaviour
{
    private enum PanelState { Playable, Completed, Locked }

    [Header("Root")]
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _dim;
    [SerializeField] private Button _dimButton;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private Button _closeButton;

    [Header("Header")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;

    [Header("Weapon power")]
    [SerializeField] private Image _classIcon;
    [SerializeField] private TMP_Text _classText;
    [SerializeField] private Image _powerFill;
    [SerializeField] private RectTransform _powerMinMarker;
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private TMP_Text _statusText;

    [Header("Enemies")]
    [SerializeField] private GameObject _rosterRoot;
    [SerializeField] private EnemyShotsRowView[] _rosterRows;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("Reward")]
    [SerializeField] private GameObject _rewardRoot;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private Image _goldIcon;
    [SerializeField] private GameObject _essenceRoot;
    [SerializeField] private TMP_Text _essenceText;
    [SerializeField] private Image _essenceIcon;

    [Header("Actions")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TMP_Text _upgradeLabel;
    [SerializeField] private Button _guildButton;

    [Header("Wiring")]
    [SerializeField] private PlayMissionPresenter _playPresenter;
    [SerializeField] private WeaponClassIconLibrary _classIcons;
    [SerializeField] private CurrencyVisualLibrary _currencyVisuals;
    [SerializeField] private string _weaponScreenId = "weapons";

    private const float HiddenOffset = 900f;
    private static readonly string HeroicPrefix = $"<color={MissionPreviewUtility.Hex(MissionPreviewUtility.GetInkHeroicColor())}>HEROIC.</color> ";

    private MainMenuServices _services;
    private Vector2 _shownPosition;
    private DG.Tweening.Sequence _anim;

    private MissionType _type;
    private MissionContext _ctx;
    private MissionGateResult _gate;
    private bool _heroic;

    public bool IsOpen => _root != null && _root.activeSelf;

    private void Awake()
    {
        if (_panel != null) _shownPosition = _panel.anchoredPosition;
        if (_root != null) _root.SetActive(false);

        _dimButton?.onClick.AddListener(Hide);
        _closeButton?.onClick.AddListener(Hide);
        _playButton?.onClick.AddListener(OnPlayClicked);
        _upgradeButton?.onClick.AddListener(OnUpgradeClicked);
        _guildButton?.onClick.AddListener(OnGuildClicked);
    }

    private bool EnsureServices()
    {
        if (_services != null) return true;
        var runtime = MainMenuRuntime.Instance;
        _services = runtime != null ? runtime.Services : null;
        return _services != null;
    }

    // ── Entry points ─────────────────────────────────────────────────────────

    public void ShowCampaignLevel(MissionData mission, int indexInZone)
    {
        if (!EnsureServices() || mission == null) return;

        var campaign = _services.Progress.CurrentZone?.GetSegmentByType(MissionType.Campaign);
        if (campaign == null) return;

        var zone = _services.Progress.CurrentZone;
        var progress = _services.ProgressData;
        bool heroic = progress.IsHeroicUnlocked();

        bool done = progress.IsCampaignLevelCleared(zone, mission);
        bool isCurrent = heroic
            ? progress.GetHeroicCurrentMission(zone) == mission
            : !campaign.IsFullyCompleted() && campaign.GetCurrentMission() == mission;

        var state = isCurrent ? PanelState.Playable : done ? PanelState.Completed : PanelState.Locked;
        _heroic = heroic;

        Present(MissionType.Campaign, mission, state,
            title: heroic ? $"HEROIC {indexInZone + 1}" : $"LEVEL {indexInZone + 1}",
            subtitle: $"{zone.ZoneName} · {mission.Name}");
    }

    /// <summary>Sniper / Boss landmark: the segment's current mission.</summary>
    public void ShowMode(MissionType type)
    {
        if (!EnsureServices()) return;

        var segment = _services.Progress.CurrentZone?.GetSegmentByType(type);
        var mission = segment?.GetCurrentMission();
        if (mission == null) return;

        string title = type switch
        {
            MissionType.Sniper => $"SNIPER {IndexOf(segment, mission) + 1}/{segment.Missions.Count}",
            MissionType.Boss => "BOSS",
            _ => type.ToString().ToUpperInvariant()
        };

        _heroic = false;
        Present(type, mission, PanelState.Playable, title, mission.Name);
    }

    private static int IndexOf(MissionSegmentData segment, MissionData mission)
    {
        for (int i = 0; i < segment.Missions.Count; i++)
            if (segment.Missions[i] == mission) return i;
        return 0;
    }

    // ── Render ───────────────────────────────────────────────────────────────

    private void Present(MissionType type, MissionData mission, PanelState state, string title, string subtitle)
    {
        _type = type;
        _ctx = _services.BuildContextFor(type, mission);
        _gate = GetGate(type, _ctx);

        var cls = _ctx.RequiredWeaponClass;
        var avail = _services.Availability.GetAvailability(_ctx);

        string lockedReason = state == PanelState.Locked ? "Finish the previous level first." : GetLockedReason(avail);
        if (lockedReason != null) state = PanelState.Locked;

        if (_titleText != null) _titleText.text = title;
        if (_subtitleText != null) _subtitleText.text = subtitle;

        // Weapon
        if (_classIcon != null && _classIcons != null && _classIcons.TryGet(cls, out var icon)) _classIcon.sprite = icon;
        if (_classText != null) _classText.text = cls.ToString().ToUpperInvariant();

        float target = 0f;
        if (_gate != null)
        {
            target = _gate.RecommendedDamage > 0 ? Mathf.Clamp01(_gate.CurrentDamage / _gate.RecommendedDamage) : 1f;
            var color = MissionPreviewUtility.GetStatusColor(_gate.Status);

            if (_powerFill != null) _powerFill.color = color;
            if (_powerText != null) _powerText.text = $"{_gate.CurrentDamage:0} / {_gate.RecommendedDamage:0}";

            if (_powerMinMarker != null)
            {
                bool soft = type == MissionType.Campaign && _gate.RequiredDamage < _gate.RecommendedDamage;
                _powerMinMarker.gameObject.SetActive(soft);
                if (soft)
                {
                    float x01 = _gate.RequiredDamage / _gate.RecommendedDamage;
                    _powerMinMarker.anchorMin = _powerMinMarker.anchorMax = new Vector2(x01, 0.5f);
                    _powerMinMarker.anchoredPosition = Vector2.zero;
                }
            }
        }

        if (_statusText != null)
            _statusText.text = state switch
            {
                PanelState.Completed => _heroic
                    ? $"<color={MissionPreviewUtility.Hex(MissionPreviewUtility.GetInkStatusColor(MissionGateStatus.Green))}>Heroic cleared!</color> The zone starts over after its last level."
                    : $"<color={MissionPreviewUtility.Hex(MissionPreviewUtility.GetInkStatusColor(MissionGateStatus.Green))}>Completed!</color> Replay it on the Guild Board for gold and essence.",
                PanelState.Locked => lockedReason,
                _ => (_heroic ? HeroicPrefix : "") + GetStatusLine(type, _gate)
            };

        // Enemies
        bool roster = type == MissionType.Campaign && _gate != null && _rosterRows != null;
        if (_rosterRoot != null) _rosterRoot.SetActive(roster);
        if (roster)
        {
            int r = Mathf.Max(1, Mathf.FloorToInt(_gate.RecommendedDamage));
            int dmg = Mathf.FloorToInt(_gate.CurrentDamage);
            for (int i = 0; i < _rosterRows.Length; i++)
            {
                bool has = i < MissionPreviewUtility.RosterTypes.Length;
                _rosterRows[i].gameObject.SetActive(has);
                if (has) _rosterRows[i].Bind(MissionPreviewUtility.GetShots(_services.Balance, r, dmg, MissionPreviewUtility.RosterTypes[i]));
            }
        }

        if (_descriptionText != null)
        {
            _descriptionText.gameObject.SetActive(!roster);
            _descriptionText.text = type switch
            {
                MissionType.Sniper => "Long-range Crossbow hunt. The best source of Crossbow essence.",
                MissionType.Boss => "Defeat the boss with your Crossbow to open the next zone.",
                _ => mission.Description
            };
        }

        // Reward
        bool showReward = state != PanelState.Completed && _services.Rewards != null;
        if (_rewardRoot != null) _rewardRoot.SetActive(showReward);
        if (showReward) RenderReward(_services.Rewards.Calculate(_ctx));

        // Actions
        bool playable = state == PanelState.Playable && _gate != null && _gate.Status != MissionGateStatus.Red;
        bool needsPower = state != PanelState.Completed && _gate != null && _gate.Status != MissionGateStatus.Green;

        if (_playButton != null) _playButton.gameObject.SetActive(playable);
        if (_guildButton != null) _guildButton.gameObject.SetActive(state == PanelState.Completed && _services.ContractBoard != null);
        if (_upgradeButton != null) _upgradeButton.gameObject.SetActive(needsPower);
        if (_upgradeLabel != null)
            _upgradeLabel.text = _gate != null && !_gate.CanUpgradeToPass ? $"NEW {cls.ToString().ToUpperInvariant()}" : "UPGRADE";

        Open(target);
    }

    private void RenderReward(MissionReward reward)
    {
        if (_goldText != null) _goldText.text = $"+{reward.Money}";
        if (_goldIcon != null && _currencyVisuals != null && _currencyVisuals.TryGet(CurrencyType.Cash, out _, out var goldIcon))
            _goldIcon.sprite = goldIcon;

        int bestClass = -1, bestAmount = 0;
        if (reward.Tokens != null)
            for (int i = 0; i < reward.Tokens.Length; i++)
                if (reward.Tokens[i] > bestAmount) { bestAmount = reward.Tokens[i]; bestClass = i; }

        if (_essenceRoot != null) _essenceRoot.SetActive(bestClass >= 0);
        if (bestClass < 0) return;

        var cls = (WeaponClass)bestClass;
        if (_essenceText != null) _essenceText.text = $"+{bestAmount} {cls} essence";
        if (_essenceIcon != null && _classIcons != null && _classIcons.TryGet(cls, out var icon)) _essenceIcon.sprite = icon;
    }

    private MissionGateResult GetGate(MissionType type, MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid || _services.Gate == null) return null;

        return type switch
        {
            MissionType.Campaign => _services.Gate.CheckCampaignGate(ctx),
            MissionType.Sniper => _services.Gate.CheckSniperGate(ctx),
            MissionType.Boss => _services.Gate.CheckBossGate(ctx),
            _ => null
        };
    }

    private static string GetLockedReason(MissionAvailability avail) => avail.Reason switch
    {
        AvailabilityBlockReason.BossNotUnlocked => "Finish every Campaign level of this zone to face the boss.",
        AvailabilityBlockReason.SniperLockedByCompanyLevel => "Opens later in the Campaign. Keep going!",
        AvailabilityBlockReason.ContractsLockedByCompanyLevel => "Opens later in the Campaign. Keep going!",
        AvailabilityBlockReason.NoMissionSelected => "Nothing to play here yet.",
        _ => null
    };

    private static string GetStatusLine(MissionType type, MissionGateResult gate)
    {
        if (gate == null) return "";

        string hex = MissionPreviewUtility.Hex(MissionPreviewUtility.GetInkStatusColor(gate.Status));

        if (type != MissionType.Campaign)
            return gate.Status == MissionGateStatus.Red
                ? $"<color={hex}>Crossbow too weak:</color> needs {gate.RequiredDamage:0} damage."
                : $"<color={hex}>Crossbow ready!</color>";

        return gate.Status switch
        {
            MissionGateStatus.Green => $"<color={hex}>Strong!</color> Every enemy falls to one shot.",
            MissionGateStatus.Yellow => $"<color={hex}>Playable.</color> Some enemies take 2 body shots: aim for the head!",
            _ => $"<color={hex}>Too weak:</color> reach {gate.RequiredDamage:0} damage to start."
        };
    }

    // ── Animation ────────────────────────────────────────────────────────────

    private void Open(float powerTarget)
    {
        _anim?.Kill();
        _root.SetActive(true);

        if (_dim != null) { var c = _dim.color; c.a = 0f; _dim.color = c; }
        if (_panel != null) _panel.anchoredPosition = _shownPosition + new Vector2(HiddenOffset, 0f);
        if (_powerFill != null) _powerFill.fillAmount = 0f;

        _anim = DOTween.Sequence().SetLink(_root);
        if (_dim != null) _anim.Join(_dim.DOFade(0.55f, 0.25f));
        if (_panel != null) _anim.Join(_panel.DOAnchorPos(_shownPosition, 0.45f).SetEase(Ease.OutBack, 0.8f));
        if (_powerFill != null) _anim.Insert(0.3f, _powerFill.DOFillAmount(powerTarget, 0.6f).SetEase(Ease.OutCubic));

        if (_rosterRows != null && _rosterRoot != null && _rosterRoot.activeSelf)
            for (int i = 0; i < _rosterRows.Length; i++)
            {
                var row = _rosterRows[i].transform;
                row.localScale = new Vector3(1f, 0f, 1f);
                _anim.Insert(0.35f + i * 0.08f, row.DOScaleY(1f, 0.25f).SetEase(Ease.OutBack));
            }

        if (_playButton != null && _playButton.gameObject.activeSelf)
        {
            var t = _playButton.transform;
            t.localScale = Vector3.one;
            _anim.Append(t.DOPunchScale(Vector3.one * 0.08f, 0.35f, 6, 0.5f));
        }
    }

    public void Hide()
    {
        if (!IsOpen) return;

        _anim?.Kill();
        _anim = DOTween.Sequence().SetLink(_root);
        if (_dim != null) _anim.Join(_dim.DOFade(0f, 0.2f));
        if (_panel != null) _anim.Join(_panel.DOAnchorPos(_shownPosition + new Vector2(HiddenOffset, 0f), 0.25f).SetEase(Ease.InBack, 0.8f));
        _anim.OnComplete(() => _root.SetActive(false));
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    private void OnPlayClicked()
    {
        if (_services == null) return;

        _services.Progress.SelectMissionType(_type);
        _services.NotifyMenuStateChanged();

        _root.SetActive(false);
        if (_playPresenter != null) _playPresenter.PlaySelected();
    }

    private void OnUpgradeClicked()
    {
        if (_ctx == null || !ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

        var highlight = _gate != null && !_gate.CanUpgradeToPass ? WeaponHighlightMode.Buy : WeaponHighlightMode.Upgrade;
        int companyLevel = _services.ProgressData.GetCompanyLevel();

        _root.SetActive(false);
        nav.Open(_weaponScreenId, new WeaponSelectionArgs(_ctx.RequiredWeaponClass, companyLevel, null, highlight), reuseCached: true);
    }

    private void OnGuildClicked()
    {
        _root.SetActive(false);
        GuildBoardNavigation.Open();
    }
}
