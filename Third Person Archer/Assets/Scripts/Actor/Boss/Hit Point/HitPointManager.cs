using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class HitPointManager : System
    {
        [SerializeField] protected HitPoint[] _hitPoints;

        public void ActivatePoint(int index) => ActivatePoint(index, true);
        public void DesactivatePoint(int index) => ActivatePoint(index, false);

        public void ActivatePoint(int index, bool value)
        {
            if (_hitPoints.Length <= index)
                return;

            _hitPoints[index].Activate(value);
        }

        public bool TryGetHitPoint(int index, out HitPoint point)
        {
            point = null;

            Debug.Log("AAAAAAAAAA " + _hitPoints.Length);

            if (_hitPoints.Length <= index)
                return false;

            Debug.Log("XXXXXXXXXX");

            point = _hitPoints[index];
            return true;
        }
    }
}