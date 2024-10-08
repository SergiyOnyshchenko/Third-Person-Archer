using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockable 
{
    public event Action OnLocked;
    public event Action OnUnlocked;
}
