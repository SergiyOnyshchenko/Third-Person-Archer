using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IMover
{
    public void Move(Transform destination, UnityAction onCompleted = null);
    public void Move(Vector3 position, UnityAction onCompleted = null);
}
