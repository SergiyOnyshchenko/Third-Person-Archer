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
        if (DataManager.Instance.TryGetData(out MissionProgressData missionProgressData))
        {
            string name = missionProgressData.Mission.Name;

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
