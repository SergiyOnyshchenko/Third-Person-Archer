using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation.Body
{
    public class BodyParentsCreator : MonoBehaviour
    {
        [SerializeField] private Transform _mainParent;
        [Header("Bones")]
        [SerializeField] private Transform _pelvis;
        [SerializeField] private Transform _spine;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftFoot;
        [SerializeField] private Transform _rightFoot;
        private BodyPartIK[] _ikPoints;
        private ParentBodyPartIK[] _ikParents;

        [ContextMenu("1 Spawn Parents")]
        public void SpawnParents()
        {
            TryDeleteExistedParents();

            CreateParent(BodyNameIK.Pelvis, _pelvis);
            CreateParent(BodyNameIK.Spine, _spine);
            CreateParent(BodyNameIK.Head, _head);
            CreateParent(BodyNameIK.LeftHand, _leftHand);
            CreateParent(BodyNameIK.RightHand, _rightHand);
            CreateParent(BodyNameIK.LeftFoot, _leftFoot);
            CreateParent(BodyNameIK.RightFoot, _rightFoot);

            _ikPoints = _mainParent.GetComponentsInChildren<BodyPartIK>();
            _ikParents = _mainParent.GetComponentsInChildren<ParentBodyPartIK>();

            foreach (var parent in _ikParents)
            {
                foreach (var point in _ikPoints)
                {
                    if (parent.Name == point.Name)
                    {
                        if (point.Name == BodyNameIK.LeftFoot || point.Name == BodyNameIK.RightFoot)
                        {
                            Vector3 footPos = parent.transform.localPosition;
                            footPos.y = 0;
                            parent.transform.localPosition = footPos;
                        }

                        point.transform.SetParent(parent.transform);
                        point.transform.localPosition = Vector3.zero;
                        point.transform.localEulerAngles = Vector3.zero;
                    }
                }
            }

            var lookAtIk = GetBodyPartIK(BodyNameIK.LookAt);
            var parentHeadIk = GetParent(BodyNameIK.Head);

            lookAtIk.transform.SetParent(parentHeadIk.transform);
            lookAtIk.transform.localPosition = Vector3.forward;
            lookAtIk.transform.localEulerAngles = Vector3.zero;
        }

        [ContextMenu("2 Set Hierarhy Normal")]
        public void SetHierarhyNormal()
        {
            _ikPoints = _mainParent.GetComponentsInChildren<BodyPartIK>();
            _ikParents = _mainParent.GetComponentsInChildren<ParentBodyPartIK>();

            var pelvisIk =  GetBodyPartIK(BodyNameIK.Pelvis);
            var spineIk =  GetBodyPartIK(BodyNameIK.Spine);

            var parentSpineIk = GetParent(BodyNameIK.Spine);
            parentSpineIk.transform.parent = pelvisIk.IkPoint;

            var parentHeadIk = GetParent(BodyNameIK.Head);
            parentHeadIk.transform.parent = spineIk.IkPoint;

            var parentLeftHandIk = GetParent(BodyNameIK.LeftHand);
            parentLeftHandIk.transform.parent = spineIk.IkPoint;

            var parentRightHandIk = GetParent(BodyNameIK.RightHand);
            parentRightHandIk.transform.parent = spineIk.IkPoint;
        }   

        [ContextMenu("3 Set Hierarhy Flat")]
        public void SetHierarhyFlat()
        {
            _ikParents = _mainParent.GetComponentsInChildren<ParentBodyPartIK>();

            foreach (var parent in _ikParents)
                parent.transform.parent = _mainParent;
        }

        private ParentBodyPartIK CreateParent(BodyNameIK ikName, Transform bone)
        {
            GameObject ikParentGameObject = new GameObject("PARENT_IK_" + ikName);

            ParentBodyPartIK ikParent = ikParentGameObject.AddComponent<ParentBodyPartIK>();
            ikParent.Init(ikName);

            ikParent.transform.position = bone.transform.position;
            ikParent.transform.SetParent(_mainParent);

            return ikParent;
        }

        private BodyPartIK GetBodyPartIK(BodyNameIK ikName)
        {
            _ikPoints = _mainParent.GetComponentsInChildren<BodyPartIK>();

            foreach (var ikPoint in _ikPoints)
            {
                if (ikPoint.Name == ikName)
                    return ikPoint;
            }

            return null;
        }

        private ParentBodyPartIK GetParent(BodyNameIK ikName)
        {
            _ikParents = _mainParent.GetComponentsInChildren<ParentBodyPartIK>();

            foreach (var ikParent in _ikParents)
            {
                if (ikParent.Name == ikName)
                    return ikParent;
            }

            return null;
        }

        private void TryDeleteExistedParents()
        {
            BodyPartIK[] ikPoints = _mainParent.GetComponentsInChildren<BodyPartIK>();
            ParentBodyPartIK[] ikParents = _mainParent.GetComponentsInChildren<ParentBodyPartIK>();

            foreach (var ikPoint in ikPoints)
                ikPoint.transform.SetParent(_mainParent);

            for (int i = ikParents.Length - 1; i >= 0; i--)
                DestroyImmediate(ikParents[i].gameObject);
        }

    }
}
