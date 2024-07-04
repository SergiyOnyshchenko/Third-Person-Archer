using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReciever 
{
    public event Action<int> OnDamaged;
    public delegate int OnTryDamaged(int damage);
    public OnTryDamaged TryDamagedCallback { get; set; }
} 
