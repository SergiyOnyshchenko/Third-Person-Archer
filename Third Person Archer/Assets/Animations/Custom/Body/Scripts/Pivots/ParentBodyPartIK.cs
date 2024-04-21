using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class ParentBodyPartIK : MonoBehaviour
    {
        [SerializeField] private BodyNameIK _name;
        public BodyNameIK Name { get => _name; }

        public void Init(BodyNameIK nameIK)
        {
            _name = nameIK;
        }
    }
}