using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponLookRotator : MonoBehaviour
{
    [SerializeField] private GameObject _aimerHodler;
    //private IAimer _aimer;

    private void Awake() 
    {
        //_aimer = _aimerHodler.GetComponent<IAimer>();
    }

    public void Update() 
    {
        //transform.rotation = Quaternion.LookRotation(_aimer.Direction, Vector3.up);
    }
}