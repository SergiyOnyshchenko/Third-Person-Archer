using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One card on the Guild Board.</summary>
public sealed class GuildContractCardView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _zoneText;
    [SerializeField] private TMP_Text _missionText;
    [SerializeField] private TMP_Text _orderText;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private GameObject _selected;

    public event Action Clicked;

    private void Awake()
    {
        if (_button != null)
            _button.onClick.AddListener(() => Clicked?.Invoke());
    }

    public void Bind(ContractCard card, string reward, bool selected)
    {
        gameObject.SetActive(card != null);
        if (card == null) return;

        if (_zoneText != null) _zoneText.text = GetZoneName(card.ZoneIndex);
        if (_missionText != null) _missionText.text = card.Mission.Name;
        if (_orderText != null) _orderText.text = $"Order: {card.OrderClass}";
        if (_rewardText != null) _rewardText.text = reward;
        if (_selected != null) _selected.SetActive(selected);
    }

    private static string GetZoneName(int zoneIndex)
    {
        var zones = MissionProgressData.Active != null ? MissionProgressData.Active.AllZones : null;
        var zone = zones != null && zoneIndex >= 0 && zoneIndex < zones.Count ? zones[zoneIndex] : null;
        return zone != null && !string.IsNullOrEmpty(zone.ZoneName) ? zone.ZoneName : $"Zone {zoneIndex + 1}";
    }
}
