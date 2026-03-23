using UnityEngine;

public class TorchIndicator : MonoBehaviour, IOutpostIndicator
{
    [SerializeField] private ParticleSystem _flameParticle;

    public void Activate()
    {
        if (_flameParticle != null)
            _flameParticle.Play();
    }

    public void Deactivate()
    {
        if (_flameParticle != null)
            _flameParticle.Stop();
    }
}