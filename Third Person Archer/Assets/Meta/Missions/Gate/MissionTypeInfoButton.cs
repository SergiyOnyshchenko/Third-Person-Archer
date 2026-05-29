using Meta.Economy;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to an info/help button (e.g., a "?" button) near a mission type tab.
/// On click, shows a MissionInfoPopup with the reward types and a short description
/// for the selected mission type.
///
/// Uses MissionInfoPopupController (not CelebrationPopup) — the popup is
/// informational, stays open until the player explicitly closes it, and shows
/// reward icons rather than a generic text block.
///
/// Setup:
/// 1. Add a Button GameObject near each mission type tab in the main menu scene.
/// 2. Attach this component.
/// 3. Set _missionType to the matching MissionType (Campaign, Contracts, Sniper, Boss).
/// 4. Wire _button, or leave null — GetComponent&lt;Button&gt;() is used as fallback.
/// 5. Ensure "MissionInfoPopup" is registered in !ScreenRegistry Popups.asset
///    pointing to the Mission Info Popup Screen prefab.
/// </summary>
public sealed class MissionTypeInfoButton : MonoBehaviour
{
    [SerializeField] private MissionType _missionType = MissionType.Campaign;
    [SerializeField] private Button _button;

    [Tooltip("Must match the ScreenRegistry ID for MissionInfoPopupController prefab.")]
    [SerializeField] private string _popupId = "MissionInfoPopup";

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button != null)
            _button.onClick.AddListener(OnInfoClicked);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnInfoClicked);
    }

    private void OnInfoClicked()
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        nav.ShowPopup(_popupId, BuildArgs(_missionType));
    }

    private static MissionInfoPopupArgs BuildArgs(MissionType type)
    {
        switch (type)
        {
            case MissionType.Campaign:
                return new MissionInfoPopupArgs(
                    title: "Campaign",
                    description: "Story mode. Progress through zones and unlock Boss fights at the end of each zone.",
                    rewardTypes: new[]
                    {
                        CurrencyType.Cash,
                        CurrencyType.BowToken,
                        CurrencyType.SpearToken,
                        CurrencyType.ShurikenToken,
                        CurrencyType.BoomerangToken,
                    },
                    rewardNote: "Token type matches the mission's weapon class"
                );

            case MissionType.Contracts:
                return new MissionInfoPopupArgs(
                    title: "Contracts",
                    description: "Repeatable missions. Replay cleared Campaign missions and earn tokens for all weapon classes every run.",
                    rewardTypes: new[]
                    {
                        CurrencyType.Cash,
                        CurrencyType.BowToken,
                        CurrencyType.CrossbowToken,
                        CurrencyType.SpearToken,
                        CurrencyType.ShurikenToken,
                        CurrencyType.BoomerangToken,
                    },
                    rewardNote: "All token classes earned every run"
                );

            case MissionType.Sniper:
                return new MissionInfoPopupArgs(
                    title: "Sniper",
                    description: "Crossbow one-shot missions. Earn Crossbow Tokens to upgrade your Crossbow and unlock Boss gates.",
                    rewardTypes: new[]
                    {
                        CurrencyType.Cash,
                        CurrencyType.CrossbowToken,
                    },
                    rewardNote: "Best source of Crossbow Tokens"
                );

            case MissionType.Boss:
                return new MissionInfoPopupArgs(
                    title: "Boss",
                    description: "Zone finale. Defeat the Boss to open the next zone. Requires Crossbow Power.",
                    rewardTypes: new[]
                    {
                        CurrencyType.Cash,
                        CurrencyType.BowToken,
                        CurrencyType.CrossbowToken,
                        CurrencyType.SpearToken,
                        CurrencyType.ShurikenToken,
                        CurrencyType.BoomerangToken,
                    },
                    rewardNote: "Large reward on defeat"
                );

            default:
                return new MissionInfoPopupArgs(
                    title: "Mission Type",
                    description: "Select a mission type to see details.",
                    rewardTypes: System.Array.Empty<CurrencyType>()
                );
        }
    }
}
