using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IShootEvent 
{
    event Action OnShooted;
}
