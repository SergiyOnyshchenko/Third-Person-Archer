using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMover
{
    public void Move(Transform destination);
    public void Move(Vector3 position);
}
