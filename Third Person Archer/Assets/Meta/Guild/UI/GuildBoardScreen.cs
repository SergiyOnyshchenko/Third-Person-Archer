using System;
using System.Text;

using Meta.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Guild Board (Contracts): 3 cards, weapon class choice, reward preview, guild rank bar,
/// daily bonus and board refresh (free once a day, then rewarded ad).
/// </summary>
public sealed class GuildBoardScreen : MonoBehaviour
{
    [Header("Rank")]
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _reputationText;
    [SerializeField] private Image _reputationFill;
    [SerializeField] private Button _claimButton;
    [SerializeField] private TMP_Text _claimLabel;

    [Header("Board")]
    [SerializeField] private GuildContractCardView[] _cards;
    [SerializeField] private TMP_Text _emptyText;
    [SerializeField] private TMP_Text _dailyText;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private TMP_Text _refreshLabel;

    [Header("Weapon")]
    [Tooltip("One per WeaponClass, in enum order.")]
    [SerializeField] private GuildClassButtonView[] _classButtons;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _playButton;

    private MainMenuServices _services;
    private IContractBoardService _board;
    private int _selectedSlot = -1;
    private WeaponClass _selectedClass;
    private bool _classChosenByPlayer;

    private void Awake()
    {
        for (int i = 0; i < _cards.Length; i++)
        {
            int slot = i;
            if (_cards[i] != null) _cards[i].Clicked += () => SelectCard(slot);
        }

        for (int i = 0; i < _classButtons.Length; i++)
        {
            var cls = (WeaponClass)i;
            if (_classButtons[i] != null) _classButtons[i].Clicked += () => SelectClass(cls);
        }

        if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
        if (_refreshButton != null) _refreshButton.onClick.AddListener(OnRefreshClicked);
        if (_claimButton != null) _claimButton.onClick.AddListener(OnClaimClicked);
    }

    private void OnEnable()
    {
        _services = MainMenuRuntime.Instance != null ? MainMenuRuntime.Instance.Services : null;
        _board = _services?.ContractBoard;
        if (_board == null) return;

        _board.EnsureFilled();
        _classChosenByPlayer = false;
        SelectCard(FirstCardSlot());
    }

    // ── Selection ────────────────────────────────────────────────────────────

    private void SelectCard(int slot)
    {
        _selectedSlot = slot;

        // Default weapon = order class; keep the player's own pick once they made one.
        var card = _board.GetCard(slot);
        if (card != null && (!_classChosenByPlayer || !_board.IsClassPlayable(_selectedClass)))
            _selectedClass = _board.IsClassPlayable(card.OrderClass) ? card.OrderClass : FirstPlayableClass();

        Render();
    }

    private void SelectClass(WeaponClass cls)
    {
        if (!_board.IsClassPlayable(cls)) return;

        _selectedClass = cls;
        _classChosenByPlayer = true;
        Render();
    }

    private int FirstCardSlot()
    {
        for (int i = 0; i < _board.SlotCount; i++)
            if (_board.GetCard(i) != null) return i;
        return -1;
    }

    private WeaponClass FirstPlayableClass()
    {
        foreach (WeaponClass cls in Enum.GetValues(typeof(WeaponClass)))
            if (_board.IsClassPlayable(cls)) return cls;
        return WeaponClass.Bow;
    }

    // ── Render ───────────────────────────────────────────────────────────────

    private void Render()
    {
        if (_board == null) return;

        RenderRank();

        bool anyCard = false;
        for (int i = 0; i < _cards.Length; i++)
        {
            var card = i < _board.SlotCount ? _board.GetCard(i) : null;
            anyCard |= card != null;

            string reward = card != null && _board.IsClassPlayable(card.OrderClass)
                ? FormatReward(card, card.OrderClass)
                : "";
            _cards[i]?.Bind(card, reward, i == _selectedSlot);
        }

        if (_emptyText != null)
        {
            _emptyText.gameObject.SetActive(!anyCard);
            _emptyText.text = "Complete a Campaign level to get Guild contracts.";
        }

        var selected = _board.GetCard(_selectedSlot);
        for (int i = 0; i < _classButtons.Length; i++)
        {
            var cls = (WeaponClass)i;
            bool playable = _board.IsClassPlayable(cls);
            string label = playable ? $"{cls}\n{_board.GetPlayerDamage(cls)} dmg" : $"{cls}\nlocked";
            _classButtons[i]?.Bind(label, playable, cls == _selectedClass, selected != null && selected.OrderClass == cls);
        }

        if (_rewardText != null)
            _rewardText.text = selected != null ? FormatReward(selected, _selectedClass) : "";

        if (_playButton != null)
            _playButton.interactable = selected != null && _board.IsClassPlayable(_selectedClass);

        int dailyLeft = _board.DailyBonusLeft;
        if (_dailyText != null)
            _dailyText.text = dailyLeft > 0 ? $"Daily bonus x2: {dailyLeft} left" : "Daily bonus used. Back tomorrow!";

        if (_refreshLabel != null)
            _refreshLabel.text = _board.IsFreeRefreshAvailable ? "New cards (free)" : "New cards (AD)";
    }

    private void RenderRank()
    {
        if (_rankText != null) _rankText.text = $"Guild rank {_board.Rank}";

        int need = _board.RepForNextRank;
        if (_reputationText != null)
            _reputationText.text = need > 0 ? $"{_board.RepIntoRank} / {need}" : "Max rank";

        if (_reputationFill != null)
            _reputationFill.fillAmount = need > 0 ? Mathf.Clamp01((float)_board.RepIntoRank / need) : 1f;

        bool pending = _board.TryGetPendingRankReward(out var reward);
        if (_claimButton != null) _claimButton.gameObject.SetActive(pending);
        if (pending && _claimLabel != null)
            _claimLabel.text = reward.Type switch
            {
                GuildRankRewardType.Gold => $"Rank {reward.Rank}: claim {reward.Amount} Gold",
                GuildRankRewardType.EssenceChest => $"Rank {reward.Rank}: claim {reward.Amount} {_selectedClass} essence",
                GuildRankRewardType.Skin => $"Rank {reward.Rank}: claim skin {reward.Skin.DisplayName}",
                _ => $"Rank {reward.Rank}: claim"
            };
    }

    private string FormatReward(ContractCard card, WeaponClass cls)
    {
        var reward = _board.GetRewardPreview(card, cls);
        var run = _board.BuildRunInfo(card, cls);

        var sb = new StringBuilder();
        sb.Append($"+{reward.Cash} Gold");

        int tokens = reward.TokensPerType != null && (int)cls < reward.TokensPerType.Length ? reward.TokensPerType[(int)cls] : 0;
        if (tokens > 0) sb.Append($"  +{tokens} {cls} essence");

        sb.Append($"  +{_board.GetReputationPreview(card, cls)} rep");

        if (run.OrderMatched) sb.Append("\nOrder bonus");
        if (run.DailyBonus) sb.Append(run.OrderMatched ? " + daily x2" : "\nDaily x2");

        return sb.ToString();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    private void OnPlayClicked()
    {
        var card = _board.GetCard(_selectedSlot);
        if (card == null || _services == null) return;

        var result = _services.MissionStart.TryStartContract(card, _selectedClass);
        if (!result.Started)
        {
            Debug.LogWarning("Guild Board: cannot start contract: " + (result.Availability?.Reason.ToString() ?? result.FailReason.ToString()));
            return;
        }

        ScenesLoader.Instance.LoadScene(result.MissionToLoad.Scene.ScenePath);
    }

    private void OnRefreshClicked()
    {
        if (_board.IsFreeRefreshAvailable)
        {
            _board.RefreshAll(free: true);
            SelectCard(FirstCardSlot());
            return;
        }

        if (_refreshButton != null) _refreshButton.interactable = false;

        Meta.Ads.Ads.Service.ShowRewarded(Meta.Ads.AdPlacements.GuildRefresh, rewarded =>
        {
            if (_refreshButton != null) _refreshButton.interactable = true;
            if (!rewarded || this == null) return;

            _board.RefreshAll(free: false);
            SelectCard(FirstCardSlot());
        });
    }

    private void OnClaimClicked()
    {
        if (_board.ClaimPendingRankReward(_selectedClass))
            Render();
    }
}
