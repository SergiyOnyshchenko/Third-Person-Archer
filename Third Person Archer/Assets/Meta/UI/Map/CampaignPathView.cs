using System.Collections.Generic;
using DG.Tweening;
using Meta.Weapons;
using Meta.Weapons.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Path of one zone (Campaign levels + Sniper levels) drawn as a dotted trail of medallions on the zone map.
/// Node positions are the children of <see cref="_pointsRoot"/> (edit them in the prefab).
/// Tapping a node opens the mission panel for that level.
/// </summary>
public sealed class CampaignPathView : MonoBehaviour
{
    [SerializeField] private int _zoneIndex;
    [SerializeField] private RectTransform _pointsRoot;
    [Tooltip("Optional end of the trail after the last level (e.g. the Boss landmark).")]
    [SerializeField] private RectTransform _tailTarget;

    [Header("Templates")]
    [SerializeField] private MapNodeView _nodeTemplate;
    [SerializeField] private Image _dotTemplate;
    [SerializeField] private RectTransform _dotsRoot;
    [SerializeField] private RectTransform _nodesRoot;
    [SerializeField, Min(10f)] private float _dotSpacing = 34f;
    [SerializeField, Range(0f, 0.4f)] private float _curvature = 0.12f;

    [Header("Look")]
    [SerializeField] private WeaponClassIconLibrary _classIcons;
    [SerializeField] private Color _doneDotColor = new Color(1f, 0.84f, 0.38f, 1f);
    [SerializeField] private Color _todoDotColor = new Color(0.96f, 0.9f, 0.76f, 0.7f);
    [SerializeField] private Color _heroicDotColor = new Color(1f, 0.42f, 0.32f, 1f);

    [Header("Wiring")]
    [SerializeField] private MissionPanelView _panel;

    private readonly List<MapNodeView> _nodes = new();
    private readonly List<List<Image>> _segmentDots = new();
    private MainMenuServices _services;

    public void Init(MainMenuServices services)
    {
        if (_services != null) return;
        _services = services;
        _services.OnMenuStateChanged += Refresh;

        Build();
        Refresh();
        PlayProgressReveal();
    }

    private void OnDestroy()
    {
        if (_services != null) _services.OnMenuStateChanged -= Refresh;
    }

    private MissionSegmentData GetCampaign()
    {
        var zones = _services?.ProgressData?.AllZones;
        if (zones == null || _zoneIndex < 0 || _zoneIndex >= zones.Count || zones[_zoneIndex] == null) return null;
        return zones[_zoneIndex].GetSegmentByType(MissionType.Campaign);
    }

    // ── Build ────────────────────────────────────────────────────────────────

    private void Build()
    {
        var campaign = GetCampaign();
        if (campaign == null || _pointsRoot == null || _nodeTemplate == null) return;

        _nodeTemplate.gameObject.SetActive(false);
        if (_dotTemplate != null) _dotTemplate.gameObject.SetActive(false);

        int count = Mathf.Min(campaign.Missions.Count, _pointsRoot.childCount);
        if (count < campaign.Missions.Count)
            Debug.LogWarning($"CampaignPathView: zone {_zoneIndex} has {campaign.Missions.Count} levels but {_pointsRoot.childCount} points.");

        var positions = new List<Vector2>();
        for (int i = 0; i < count; i++)
            positions.Add(((RectTransform)_pointsRoot.GetChild(i)).anchoredPosition);
        if (_tailTarget != null)
            positions.Add(_tailTarget.anchoredPosition);

        for (int i = 0; i + 1 < positions.Count; i++)
            _segmentDots.Add(BuildDots(positions[i], positions[i + 1], i % 2 == 0 ? 1f : -1f));

        for (int i = 0; i < count; i++)
        {
            int index = i;
            var node = Instantiate(_nodeTemplate, _nodesRoot != null ? _nodesRoot : transform);
            node.name = $"Level {i + 1}";
            node.gameObject.SetActive(true);
            ((RectTransform)node.transform).anchoredPosition = positions[i];
            node.Clicked += () => OnNodeClicked(index);
            _nodes.Add(node);
        }
    }

    private List<Image> BuildDots(Vector2 from, Vector2 to, float side)
    {
        var dots = new List<Image>();
        if (_dotTemplate == null) return dots;

        Vector2 delta = to - from;
        float length = delta.magnitude;
        if (length < 1f) return dots;

        Vector2 normal = new Vector2(-delta.y, delta.x) / length;
        Vector2 control = (from + to) * 0.5f + normal * (length * _curvature * side);

        int steps = Mathf.Max(1, Mathf.FloorToInt(length / _dotSpacing));
        // Skip the ends: the medallions cover them.
        for (int s = 1; s < steps; s++)
        {
            float t = s / (float)steps;
            if (t < 0.18f || t > 0.82f) continue;

            Vector2 p = (1 - t) * (1 - t) * from + 2 * (1 - t) * t * control + t * t * to;
            var dot = Instantiate(_dotTemplate, _dotsRoot != null ? _dotsRoot : transform);
            dot.gameObject.SetActive(true);
            ((RectTransform)dot.transform).anchoredPosition = p;
            dots.Add(dot);
        }

        return dots;
    }

    // ── Refresh ──────────────────────────────────────────────────────────────

    private bool IsHeroic => _services.ProgressData != null && _services.ProgressData.IsHeroicUnlocked();

    private ZoneData GetZone() => _services.ProgressData.AllZones[_zoneIndex];

    /// <summary>Level cleared in the current mode (normal campaign, or Heroic once the campaign is done).</summary>
    private bool IsCleared(MissionSegmentData campaign, int index) =>
        index < campaign.Missions.Count && _services.ProgressData.IsCampaignLevelCleared(GetZone(), campaign.Missions[index]);

    private void Refresh()
    {
        var campaign = GetCampaign();
        if (campaign == null) return;

        bool heroic = IsHeroic;
        var zone = GetZone();

        // Heroic: the current level always exists (the zone starts over after its last level).
        MissionData current = heroic ? _services.ProgressData.GetHeroicCurrentMission(zone) : campaign.GetCurrentMission();
        bool noCurrent = !heroic && campaign.IsFullyCompleted();

        for (int i = 0; i < _nodes.Count; i++)
        {
            var mission = campaign.Missions[i];
            bool isCurrent = !noCurrent && mission == current;
            bool done = IsCleared(campaign, i);

            var state = isCurrent ? MapNodeState.Current : done ? MapNodeState.Completed : MapNodeState.Locked;

            Sprite icon = null;
            Color? status = null;
            if (mission.IsSniper && _classIcons != null)
                _classIcons.TryGet(WeaponClass.Crossbow, out icon);

            if (isCurrent)
            {
                var ctx = BuildContext(zone, mission);
                if (_classIcons != null) _classIcons.TryGet(ctx.RequiredWeaponClass, out icon);

                var gate = _services.Gate.CheckCampaignGate(ctx);
                if (gate != null) status = MissionPreviewUtility.GetStatusColor(gate.Status);
            }

            _nodes[i].Bind(state, i + 1, icon, status, heroic, iconAlways: mission.IsSniper);
        }

        // Segment i goes from node i to node i+1 (or to the tail): gold once node i is completed.
        var doneColor = heroic ? _heroicDotColor : _doneDotColor;
        for (int i = 0; i < _segmentDots.Count; i++)
        {
            bool done = IsCleared(campaign, i);
            foreach (var dot in _segmentDots[i])
                dot.color = done ? doneColor : _todoDotColor;
        }
    }

    /// <summary>Context of a level of THIS path's zone (Heroic: all zones are browsable).</summary>
    private MissionContext BuildContext(ZoneData zone, MissionData mission)
    {
        var ctx = _services.BuildContextFor(MissionType.Campaign, mission);
        if (ctx.Zone == zone) return ctx;

        return new MissionContext(zone, _zoneIndex, MissionType.Campaign, mission, ctx.GlobalCampaignIndex,
            ctx.CompanyLevel, ctx.RequiredWeaponClass);
    }

    private void OnNodeClicked(int index)
    {
        var campaign = GetCampaign();
        if (campaign == null || _panel == null) return;

        _panel.ShowCampaignLevel(campaign.Missions[index], index);
    }

    /// <summary>Plays a short "trail fills" animation up to the current level (after a win).</summary>
    public void PlayProgressReveal()
    {
        var campaign = GetCampaign();
        if (campaign == null) return;

        float delay = 0f;
        for (int i = 0; i < _segmentDots.Count; i++)
        {
            if (!IsCleared(campaign, i)) break;

            foreach (var dot in _segmentDots[i])
            {
                dot.transform.localScale = Vector3.zero;
                dot.transform.DOScale(1f, 0.25f).SetDelay(delay).SetEase(Ease.OutBack).SetLink(dot.gameObject);
                delay += 0.03f;
            }
        }
    }
}
