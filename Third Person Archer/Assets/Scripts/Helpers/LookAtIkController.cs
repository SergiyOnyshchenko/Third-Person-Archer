using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using DG.Tweening;

public class LookAtIkController : MonoBehaviour
{
    [SerializeField] private LookAtIK _lookAtIK;

    private void Start()
    {
        if(_lookAtIK == null)
            _lookAtIK = GetComponent<LookAtIK>();
    }

    public void Enable()
    {
        DOVirtual.Float(0f, 1f, 1f, v => { 
            _lookAtIK.solver.bodyWeight = v;
            _lookAtIK.solver.headWeight = v;
        });
    }

    public void Disable()
    {
        DOVirtual.Float(1f, 0f, 1f, v => {
            _lookAtIK.solver.bodyWeight = v;
            _lookAtIK.solver.headWeight = v;
        });
    }
}
