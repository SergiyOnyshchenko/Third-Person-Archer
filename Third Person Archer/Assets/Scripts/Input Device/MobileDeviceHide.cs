using UnityEngine;

public class MobileDeviceHide : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(!InputDeviceManager.Instance.IsMobileDevice);    
    }
}
