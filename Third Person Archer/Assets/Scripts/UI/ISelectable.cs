using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ISelectable
{
    bool IsSelected { get; }
    public event Action OnSelected;
    public event Action OnDeselected;

}
