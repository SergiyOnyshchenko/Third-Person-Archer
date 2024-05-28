using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

namespace CustomAnimation
{
    public abstract class AnimationPoseDataCreator<T, D> : MonoBehaviour where D : class
    {
        [SerializeField] private string _poseName = "NewPose";
        [SerializeField] private string _path = "Assets/Animations/";
        [Space]
        [SerializeField] protected AnimatorController<T, D> _animator;

        [ContextMenu("Create Pose Data")]
        public virtual void CreatePoseData() 
        {
            //AssetDatabase.CreateAsset(GetNewData(), _path + _poseName + ".asset");
        }
       
        protected abstract AnimationPoseData<D> GetNewData();
    }
}
