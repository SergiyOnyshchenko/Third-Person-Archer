using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WarningPopupView : PopupView
{
    [SerializeField] private TextMeshProUGUI _messageText;

    public override void Initialize(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Close();
        }
    }
}
