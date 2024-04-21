using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Hand
{
    [System.Serializable]
    public class Finger
    {
        [SerializeField] private FingerName _name;
        [SerializeField] private Transform[] _bones;
        public FingerName Name { get => _name; }
        public FingerBone[] Bones { get; private set; }

        public void Init()
        {
            Bones = new FingerBone[_bones.Length];

            for (int i = 0; i < _bones.Length; i++)
                Bones[i] = new FingerBone(_bones[i], _name, i);
        }

        public void SetFingerManualy(FingerName fingerName, Transform parent)
        {
            _name = fingerName;
            _bones = new Transform[3];

            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i] = parent;

                if(parent.childCount != 0)
                    parent = parent.GetChild(0);
                else
                    break;
            }
        }
    }
}
