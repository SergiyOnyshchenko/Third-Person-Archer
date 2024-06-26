using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowView : MonoBehaviour, IBowView
{
    [SerializeField] private GameObject _model;
    [SerializeField] private BowSpring _bowSpring;
    [SerializeField] private GameObject _arrow;

    public GameObject Model => _model;
    public BowSpring BowSpring => _bowSpring;
    public GameObject Arrow => _arrow;
}
