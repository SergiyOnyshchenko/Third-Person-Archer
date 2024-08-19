using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;

public class Frostbite : BooleanProperty
{
    public void Freeze()
    {
        _value = true;
    }
}
