using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Actor.Properties
{
    public class PatrolPath : Property, IActorIniter
    {
        [SerializeField] private Transform[] _pathPoints;
        private Vector3 _startPoint;
        public bool HasPath => _pathPoints.Length != 0;

        public void InitActor(ActorController actor)
        {
            _startPoint = actor.transform.position;
        }

        public Vector3[] GetPath()
        {
            List<Vector3> path = new List<Vector3>();
            path.Add(_startPoint);

            foreach (var point in _pathPoints)
                path.Add(point.transform.position);
            
            return path.ToArray();
        }
    }
}