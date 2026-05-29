using System.Collections.Generic;
using Meta.Economy;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup controller for mission type info cards.
/// Shows: title, short description, a row of reward-type icons, and a reward note.
/// Stays open until the player presses the close button (no auto-close).
///
/// Implements IReceivesArgs&lt;MissionInfoPopupArgs&gt; — works with UINavigator.ShowPopup.
///
/// Setup:
/// 1. Create a prefab named "Mission Info Popup Screen.prefab" in
///    Assets/Scripts/UI New/Prefabs/PopUps/.
/// 2. Add ScreenView component (required by UINavigator).
/// 3. Add this component.
/// 4. Wire all serialized fields.
/// 5. Create a small "Mission Reward Tag.prefab" (MissionRewardTagView component,
///    Image + TMP label) and assign it to _rewardTagPrefab.
/// 6. Assign a CurrencyVisualLibrary ScriptableObject to _currencyVisuals.
/// 7. Register the popup in !ScreenRegistry Popups.asset with ID "MissionInfoPopup".
/// </summary>
public sealed class MissionInfoPopupController : MonoBehaviour, IReceivesArgs<MissionInfoPopupArgs>
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _rewardNoteText;

    [Header("Reward icons")]
    [Tooltip("Parent transform for spawned reward tag views (use Horizontal Layout Group).")]
    [SerializeField] private Transform _rewardTagsRoot;

    [Tooltip("Prefab with MissionRewardTagView component (icon + label, no amount).")]
    [SerializeField] private MissionRewardTagView _rewardTagPrefab;

    [Tooltip("ScriptableObject that maps CurrencyType to display name and icon sprite.")]
    [SerializeField] private CurrencyVisualLibrary _currencyVisuals;

    [Header("Button")]
    [SerializeField] private Button _closeButton;

    private readonly List<MissionRewardTagView> _spawnedTags = new();

    private void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnClose);
    }

    public bool ValidateArgs(MissionInfoPopupArgs args) => args != null;

    public void ApplyArgs(MissionInfoPopupArgs args)
    {
        if (_titleText != null)
            _titleText.text = args.Title;

        if (_descriptionText != null)
            _descriptionText.text = args.Description;

        bool hasNote = !string.IsNullOrEmpty(args.RewardNote);
        if (_rewardNoteText != null)
        {
            _rewardNoteText.text = hasNote ? args.RewardNote : string.Empty;
            _rewardNoteText.gameObject.SetActive(hasNote);
        }

        RebuildRewardTags(args.RewardTypes);
    }

    private void RebuildRewardTags(CurrencyType[] rewardTypes)
    {
        // Clear previous
        foreach (var tag in _spawnedTags)
        {
            if (tag != null)
                Destroy(tag.gameObject);
        }
        _spawnedTags.Clear();

        if (_rewardTagsRoot == null || _rewardTagPrefab == null || rewardTypes == null)
            return;

        foreach (var currency in rewardTypes)
        {
            var tag = Instantiate(_rewardTagPrefab, _rewardTagsRoot);
            tag.Setup(currency, _currencyVisuals);
            _spawnedTags.Add(tag);
        }
    }

    private void OnClose()
    {
        Destroy(gameObject);
    }
}
