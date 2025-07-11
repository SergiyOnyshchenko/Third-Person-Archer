using UnityEngine;

public class MissionSelectorView : MonoBehaviour
{
    [SerializeField] private MissionSelectorButton[] _buttonViews;

    public void Bind(MissionSelector selector)
    {
        foreach (var buttonView in _buttonViews)
        {
            buttonView.Bind(selector);
        }
    }
}
