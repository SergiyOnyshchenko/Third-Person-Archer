using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISoundListener
{
    public void ReciveSound(string name, float power, GameObject owner);
}
