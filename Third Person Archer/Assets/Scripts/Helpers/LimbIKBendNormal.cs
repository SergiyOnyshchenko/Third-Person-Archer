using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class LimbIKBendNormal : MonoBehaviour
{
    [SerializeField] private LimbIK _limbIK;
    [SerializeField] private Transform _target;

    [Header("Axis")]
    [SerializeField] private Vector3 _axis = Vector3.up; // (0,1,0)

    private void Reset()
    {
        _limbIK = GetComponent<LimbIK>();
    }

    private void LateUpdate()
    {
        if (_limbIK == null || _target == null)
            return;

        float xDeg = _target.localEulerAngles.x;

        // Rotation itself defines the blend:
        //  90°  -> -1
        //  270° -> +1
        float blend = -Mathf.Sin(xDeg * Mathf.Deg2Rad);

        _limbIK.solver.bendNormal = _axis.normalized * blend;
    }
}