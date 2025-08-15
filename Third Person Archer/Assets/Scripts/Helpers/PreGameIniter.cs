using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameIniter : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        ScenesLoader.Instance.LoadMainMenu();
    }
}
