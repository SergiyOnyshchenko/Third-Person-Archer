using UnityEngine;
using System.Runtime.InteropServices;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    [DllImport("__Internal")]
    private static extern int IsMobile();

    public bool IsTouchDevice { get; private set; }
    public bool IsMobileDevice { get; private set; }

    public bool UseTouchControls => IsTouchDevice || IsMobileDevice;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DetectInputType();
    }

    private void DetectInputType()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        IsMobileDevice = IsMobile() == 1;
#else
        IsMobileDevice = Application.isMobilePlatform;
#endif
        IsTouchDevice = Input.touchSupported;
    }
}