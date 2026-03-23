using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelNameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;

    private void Start()
    {
        SetLevelName();
    }

    public void SetLevelName()
    {
        if (GameplayRuntime.Instance != null && 
            GameplayRuntime.Instance.LaunchRequest != null && 
            GameplayRuntime.Instance.LaunchRequest.MissionToLoad != null)
        {
            string name = GameplayRuntime.Instance.LaunchRequest.MissionToLoad.Name;

            if (string.IsNullOrEmpty(name))
            {
                HideView();
                return;
            }

            _textField.text = name;
        }
        else
        {
            HideView();
        }
    }

    private void HideView()
    {
        gameObject.SetActive(false);
    }
}
