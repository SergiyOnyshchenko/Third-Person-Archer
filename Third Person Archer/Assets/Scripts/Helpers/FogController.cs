using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FogController : MonoBehaviour
{
    [SerializeField] private float _updateDuration;

    public void Activate(bool value)
    {
        RenderSettings.fog = value;
    }

    public void SetColor(Color color)
    {
        Color current = RenderSettings.fogColor;

        DOVirtual.Color(current, color, _updateDuration, (value) =>
        {
            RenderSettings.fogColor = value;
        });
    }

    public void SetEndDistance(float value)
    {
        DOVirtual.Float(RenderSettings.fogEndDistance, value, _updateDuration, v => RenderSettings.fogEndDistance = v);
    }
}
