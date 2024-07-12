using UnityEngine;

public class VFXController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _visualEffect;
    [SerializeField] private bool _enabledOnStart;

    private void Awake()
    {
        if (_enabledOnStart)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    public void Enable()
    {
        if (_visualEffect == null) return;

        Debug.Log("Play VFX");
        _visualEffect.gameObject.SetActive(true);
        _visualEffect.Play(true);
    }

    public void EnableSmooth()
    {
        if (_visualEffect == null) return;
    }

    public void Disable()
    {
        if (_visualEffect == null) return;

        Debug.Log("Stop VFX");
        
        _visualEffect.Stop(true);
        _visualEffect.gameObject.SetActive(false);
    }

    public void DisableSmooth()
    {
        if (_visualEffect == null) return;
    }
}
