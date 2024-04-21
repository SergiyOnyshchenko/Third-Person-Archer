using System.Collections;
using System.Collections.Generic;
//using HandAnimation;
using UnityEngine;
//using HandAnimation;

public class HandAnimationTest : MonoBehaviour
{
    //[SerializeField] private HandPoseData _pose1;
    //[SerializeField] private HandPoseData _pose2;
    //[Space]
    //[SerializeField] private HandAnimator _animator;
    
    private void Update() 
    {   
        if (Input.GetKeyDown("w"))
        {
            //_animator.DoPose(_pose1, 0.25f);
        }

        if (Input.GetKeyDown("s"))
        {
            //_animator.DoPose(_pose2, 0.25f);
        }
    }
}
