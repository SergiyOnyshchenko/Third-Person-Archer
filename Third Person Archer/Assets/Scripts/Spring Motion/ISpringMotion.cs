using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpringMotion 
{
    void SetTargetPosition(Vector3 position);
    void SetTargetRotation(Quaternion rotation);
    void SetSpringParams(float power, float dumping);
}
