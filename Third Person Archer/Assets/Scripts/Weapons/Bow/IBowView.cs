using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBowView
{
    public GameObject Model { get;  }
    public BowSpring BowSpring { get; }
    public GameObject Arrow { get; }
}
