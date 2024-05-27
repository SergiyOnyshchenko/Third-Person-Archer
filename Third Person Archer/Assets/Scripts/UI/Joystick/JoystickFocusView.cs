using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickFocusView : MonoBehaviour
{
    [SerializeField] private Image _up;
    [SerializeField] private Image _down;
    [SerializeField] private Image _left;
    [SerializeField] private Image _right;
    private Joystick _joystick;

    private void Start()
    {
        _joystick = GetComponent<Joystick>();
    }

    private void Update()
    {
        if (_joystick == null)
            return;

        float upAlpha = Mathf.Lerp(0, 1, _joystick.Direction.y);
        SetAlpha(_up, upAlpha);

        float downAlpha = Mathf.InverseLerp(0, -1, _joystick.Direction.y);
        SetAlpha(_down, downAlpha);

        float leftAlpha = Mathf.InverseLerp(0, -1, _joystick.Direction.x);
        SetAlpha(_left, leftAlpha);

        float rightAlpha = Mathf.Lerp(0, 1, _joystick.Direction.x);
        SetAlpha(_right, rightAlpha);
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
