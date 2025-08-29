using UnityEngine;

public class PlayButtonController : MonoBehaviour
{
    [SerializeField] private MetaGameController _metaGameController;
    [SerializeField] private PlayButtonView _playButtonView;

    private void Start()
    {
        _playButtonView.BindClick(OnPlayClicked);
        UpdateButtonState();
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool canPlay = _metaGameController.CanPlaySelectedMission();
        _playButtonView.SetInteractable(canPlay);
    }

    private void OnPlayClicked()
    {
        if (_metaGameController.CanPlaySelectedMission())
        {
            _metaGameController.PlaySelectedMission();
        }
    }
}
