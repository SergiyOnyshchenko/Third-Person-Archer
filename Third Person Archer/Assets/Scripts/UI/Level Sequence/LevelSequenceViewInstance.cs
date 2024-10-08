using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LevelSequenceViewInstance : MonoBehaviour
{
    [SerializeField] private GameObject _normalIcon;
    [SerializeField] private GameObject _bossIcon;
    [SerializeField] private TextMeshProUGUI _levelNumber;
    public UnityEvent OnViewSelected = new UnityEvent();

    public void Init( int levelNumber, bool isBoss, bool isCurrent)
    {
        if (isBoss)
        {
            _normalIcon.SetActive(false);
            _bossIcon.SetActive(true);
        }

        _levelNumber.text = levelNumber.ToString();

        if (isCurrent)
            OnViewSelected?.Invoke();
    }
}
