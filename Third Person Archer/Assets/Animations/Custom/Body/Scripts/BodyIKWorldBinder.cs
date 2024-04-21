using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class BodyIKWordBinder : MonoBehaviour
    {
        [SerializeField] private Transform _worldTarget;
        [Space]
        [SerializeField] private BodyIKAnimatorController _animationController;
        [SerializeField] private BodyNameIK _ikName;
        private BodyPartIK _bodyPartIK;
        
        private void Start() 
        {
            _bodyPartIK = _animationController.Body.GetBodyPart(_ikName);
        }

        private void Update() 
        {
            _bodyPartIK.transform.position = _worldTarget.transform.position;
            _bodyPartIK.transform.rotation = _worldTarget.transform.rotation;
        }
    }   
}