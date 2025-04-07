using Actor;
using UnityEngine;

public class FpvInputDecorator : FpvInput
{
    [SerializeField] private DeltaFpvInput _deltaFpvInput;
    [SerializeField] private MouseFpvInput _mouseFpvInput;

    private FpvInput _currentFpvInput;

    public override float Horizontal { get => _currentFpvInput.Horizontal; }
    public override float Vertical { get => _currentFpvInput.Vertical; }
    public override bool IsFrozen { get => _currentFpvInput.IsFrozen; }


    private void Start()
    {
        InitCurrentInput();
    }

    private void InitCurrentInput()
    {
        if (InputDeviceManager.Instance.UseTouchControls)
            SetCurrentInput(_deltaFpvInput);
        else
            SetCurrentInput(_mouseFpvInput);
    }

    private void SetCurrentInput(FpvInput fpvInput)
    {
        fpvInput.gameObject.SetActive(true);
        _currentFpvInput = fpvInput;
    }
}
