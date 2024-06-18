using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private int _damage = 1000;
    [SerializeField] private float _range = 10;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _pushPower = 10;
    public Vector3 Center => transform.position;

    public void DoExplosion()
    {
        StartCoroutine(_DoExplosion());
    }

    private IEnumerator _DoExplosion()
    {
        Collider[] hitColliders = Physics.OverlapSphere(Center, _range, _mask);

        foreach (var hitCollider in hitColliders)
            if (hitCollider.TryGetComponent(out IDamageable damager))
                damager.DoDamage(Mathf.RoundToInt(_damage));

        //yield return new WaitForSeconds(0.05f);
        yield return null;

        foreach (var hitCollider in hitColliders)
            if (hitCollider.TryGetComponent(out Rigidbody rigidbody))
            {
                yield return null;

                if (rigidbody.isKinematic)
                    continue;

                rigidbody.AddExplosionForce(_pushPower, Center, _range, 0.5f, ForceMode.Impulse);
            }
    }
}
