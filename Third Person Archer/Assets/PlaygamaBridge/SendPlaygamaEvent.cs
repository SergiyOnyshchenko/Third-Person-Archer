using System.Collections;
using System.Collections.Generic;
using Playgama.Modules.Platform;
using Playgama;
using UnityEngine;

public class SendPlaygamaEvent : MonoBehaviour
{
    [SerializeField] private PlatformMessage _message;

    public void Send()
    {
        Bridge.platform.SendMessage(_message);
    }
}
