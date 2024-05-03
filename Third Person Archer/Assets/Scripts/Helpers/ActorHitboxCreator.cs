using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ActorHitboxCreator : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;

    [ContextMenu("Create Hitboxes")]
    public void CreateHitboxes()
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();

        foreach (var body in bodies)
        {
            if (body.gameObject.TryGetComponent(out Hitbox existedHitbox))
            {
                SetLayerMask(existedHitbox);
                continue;
            }
                
            Hitbox newHitBox = body.gameObject.AddComponent<Hitbox>();
            SetLayerMask(newHitBox);
        }
    }

    private void SetLayerMask(Hitbox hitbox)
    {
        hitbox.SetCollisionMask(_layerMask);
    }
}
