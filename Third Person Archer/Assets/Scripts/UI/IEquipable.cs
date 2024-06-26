using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipable
{
    public event Action OnEquipped;
    public event Action OnUnequipped;
}
