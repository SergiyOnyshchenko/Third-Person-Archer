using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace CustomAnimation
{
    public abstract class AnimatorController<T, D> : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Transform _properiesParent;
        [SerializeField] private Transform _animatorsParent;
        [Space]
        protected Animator<T, D> _currentAnimator;
        protected AnimationProperty[] _properties;
        private Animator<T, D>[] _animators;
        public AnimationProperty[] Properties => _properties;

        protected virtual void Awake()
        {
            InitLayout();

            _properties = InitProperties();
            _animators = InitAnimators();

            SetAnimator(_animators[0]);
        }

        private void OnValidate()
        {
            CreateLayout();
        }

        public abstract void DoPose(IAnimationPose<D> pose);
        public abstract IAnimationPose<D> LerpPoses(IAnimationPose<D> pose1, IAnimationPose<D> pose2, float value);
        public abstract D[] GetPoseData();
        protected abstract AnimationProperty[] InitProperties();
        protected abstract Animator<T, D>[] InitAnimators();

        public bool TrySetAnimator<A>() where A : Animator<T, D>
        {
            foreach (var myAnimator in _animators)
            {
                if (myAnimator != null && myAnimator.GetType() == typeof(T))
                {
                    if (_currentAnimator == myAnimator)
                        return true;

                    SetAnimator(myAnimator);
                    return true;
                }
            }

            return false;
        }

        public bool TrySetAnimator(AnimatorType type)
        {
            foreach (var myAnimator in _animators)
            {
                if (myAnimator != null && myAnimator.Type == type)
                {
                    if (_currentAnimator == myAnimator)
                        return true;

                    SetAnimator(myAnimator);
                    return true;
                }
            }

            return false;
        }

        protected GameObject CreatePropertyObject(string name) => CreateLayoutObject(name + " Propery", _properiesParent);
        protected GameObject CreateAnimatorObject(string name) => CreateLayoutObject(name + " Animator", _animatorsParent);
        private GameObject CreateLayoutObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject;
        }

        private void SetAnimator(Animator<T, D> animator)
        {
            _currentAnimator = animator;
            Debug.Log(gameObject.name + " Set Animator " + animator.Type);
        }

        private void InitLayout()
        {
            if (_properiesParent == null)
            {
                if (transform.childCount >= 1)
                    _properiesParent = transform.GetChild(0);
                else
                    _properiesParent = transform;
            }

            if (_animatorsParent == null)
            {
                if (transform.childCount >= 2)
                    _animatorsParent = transform.GetChild(1);
                else
                    _animatorsParent = transform;
            }
        }

        private void CreateLayout()
        {
            if (_properiesParent == null)
                _properiesParent = CreateLayoutInstance("Properties");

            if (_animatorsParent == null)
                _animatorsParent = CreateLayoutInstance("Animators");
        }

        private Transform CreateLayoutInstance(string name)
        {
            GameObject properties = null;

            if (HasGameobjectWithName(name, transform, out properties))
            {
                _properiesParent = properties.transform;
            }
            else
            {
                properties = new GameObject(name);
                properties.transform.SetParent(transform);
            }

            return properties.transform;
        }

        private bool HasGameobjectWithName(string targetName, Transform parent, out GameObject target)
        {
            target = null;

            foreach (Transform child in parent)
            {
                if (child.name != targetName)
                    continue;

                target = child.gameObject;
                return true;
            }

            return false;
        }
    }
}