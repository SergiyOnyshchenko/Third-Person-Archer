using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventHandler : MonoBehaviour
{
    public void SendLoadNextLevel()
    {
        ScenesLoader.Instance.LoadMainMenu();
    }

    public void SendContinueLevel()
    {
        
    }

    public void SendReloadLevel()
    {
        ScenesLoader.Instance.LoadCurrentMission();
    }

    public void SendLoadMainMenu()
    {
        ScenesLoader.Instance.LoadMainMenu();
    }

    public void SendBackToMainMenuButtonPressedEvent()
    {
        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendBackToMainMenuButtonPress();
    }
}
