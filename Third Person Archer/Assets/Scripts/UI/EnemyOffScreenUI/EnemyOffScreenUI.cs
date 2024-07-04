using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class EnemyOffScreenUI : MonoBehaviour
{
    [SerializeField] private ITarget _target;
    public ITarget Target { get { return _target; } }

    public void Init(ITarget target)
    {
        _target = target;
    }
}
