using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSoundTrigger : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField] private LayerMask _targetMask;

    public void Play()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.TryGetComponent(out ISoundListener reciever))
                reciever.ReciveSound("HitSound", 1, gameObject);
        }
    }
}
