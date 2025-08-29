using UnityEngine;
using UnityEngine.UI;

public class PlayButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;

    public void SetInteractable(bool enabled)
    {
        _button.interactable = enabled;
    }

    public void BindClick(System.Action callback)
    {
        _button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            _button.onClick.AddListener(() => callback());
        }
    }
}
