using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform _target;
    
    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(_target.position - transform.position);
    }
}
