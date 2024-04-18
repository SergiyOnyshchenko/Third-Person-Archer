using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DelaySequence : Sequence
{
    [SerializeField] private float _delay;

    public override void Begin()
    {
        base.Begin();
        DOVirtual.DelayedCall(_delay, Finish);
    }
}
