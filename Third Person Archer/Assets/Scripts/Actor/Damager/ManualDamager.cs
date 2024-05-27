using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ManualDamager : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] ActorController _actorController;

    public void DoDamage()
    {
        if (_actorController.TryGetSystem(out Health health))
        {
            health.ApplyDamage(_damage);
        }
    }
}
