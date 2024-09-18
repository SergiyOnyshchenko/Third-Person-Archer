using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class ManaDropper : System, IActorIniter
    {


        [SerializeField] private ManaDrop _prefab;
        private Transform _spawnPoint;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out Target target))
                _spawnPoint = target.TargetPoint;
        }

        public void Drop()
        {
            ManaDrop manaDrop = Instantiate(_prefab, _spawnPoint.position, Quaternion.identity);
            manaDrop.Init(Player.Instance, 10);
        }
    }
}