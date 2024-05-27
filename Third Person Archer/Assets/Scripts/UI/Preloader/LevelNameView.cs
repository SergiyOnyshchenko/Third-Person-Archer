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
        if (LevelManager.Instance == null)
        {
            HideView();
            return;
        }

        string levelName = LevelManager.Instance.CurrentLevel.Name;

        if (string.IsNullOrEmpty(levelName))
        {
            HideView();
            return;
        }

        _textField.text = levelName;
    }

    private void HideView()
    {
        gameObject.SetActive(false);
    }
}
