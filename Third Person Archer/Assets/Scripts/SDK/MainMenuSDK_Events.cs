using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSDK_Events : MonoBehaviour
{
    private void Start()
    {
        SendMainMenuOpen();
    }

    public void SendMainMenuOpen()
    {
        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendMainMenuOpen();
    }

    public void SendMenuWeaponPanelOpen()
    {
        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendMenuWeaponPanelOpen();
    }

    public void SendMenuSkinPanelOpen()
    {
        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendMenuSkinPanelOpen();
    }
}
