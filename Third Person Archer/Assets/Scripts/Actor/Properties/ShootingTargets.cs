using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class ShootingTargets : Property
    {
        [SerializeField] private int _count;
        private List<ITarget> _targets = new List<ITarget>();
        public List<ITarget> Targets { get => _targets; }
        public int Count { get => _count; }

        public void Init(List<ITarget> targets)
        {
            _targets = targets;

            foreach (var target in _targets)
            {
                target.OnDied += RemoveTarget;
            }

            UpdateCount();
        }

        private void RemoveTarget(ITarget target)
        {
            target.OnDied -= RemoveTarget;
            _targets.Remove(target);
            UpdateCount();
        }

        private void UpdateCount()
        {
            _count = _targets.Count;
        }
    }
}