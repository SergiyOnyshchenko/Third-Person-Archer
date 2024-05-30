using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class FieldOfView
{
    [SerializeField] private float _maxDistance;
    [SerializeField, Range(0, 360)] private float _viewAngle;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private Transform _eyesPoint;

    public ITarget[] GetTargetsInView(ITarget[] targets)
    {
        Dictionary<ITarget, float> targetInView = new Dictionary<ITarget, float>();

        foreach (var target in targets)
            if (IsTargetInView(target, out float distance))
                targetInView.Add(target, distance);

        return targetInView.OrderBy(kp => kp.Value).Select(kp => kp.Key).ToArray();
    }

    public bool IsTargetInView(ITarget target, out float distance)
    {
        distance = Vector3.Distance(_eyesPoint.position, target.TargetPoint.position);

        if(distance > _maxDistance)
            return false;

        Vector3 direction = target.TargetPoint.position - _eyesPoint.position;

        if (Vector3.Angle(_eyesPoint.forward, direction) >= _viewAngle / 2)
            return false;

        if (Physics.Raycast(_eyesPoint.position, direction.normalized, distance, _obstacleMask))
            return false;

        return true;
    }
}
