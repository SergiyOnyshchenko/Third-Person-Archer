using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ActorTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;

    private void OnTriggerEnter(Collider other)
    {
        if ((_layerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            Transform parent = TransformExtension.GetParentWithTag(other.transform);

            if (parent.TryGetComponent(out ActorController actor))
            {
                actor.gameObject.SetActive(false);
            }
        }
    }
}
