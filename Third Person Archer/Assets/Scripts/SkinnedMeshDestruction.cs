using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using RayFire;

public class SkinnedMeshDestruction : MonoBehaviour
{
    [SerializeField] private Transform _damagePoint;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshRenderer _bakedMesh;

    void Start()
    {
        DOVirtual.DelayedCall(2, () =>
        {
            BakeMesh();
            AddRayfireComponents();
        });
    }

    private void BakeMesh()
    {
        if (skinnedMeshRenderer == null)
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        Mesh bakedMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(bakedMesh);

        // Create a GameObject with a MeshFilter and MeshRenderer
        GameObject meshObject = new GameObject("BakedMesh");
        meshObject.transform.SetParent(skinnedMeshRenderer.transform.parent);
        meshObject.transform.position = skinnedMeshRenderer.transform.position;
        meshObject.transform.rotation = skinnedMeshRenderer.transform.rotation;

        MeshFilter mf = meshObject.AddComponent<MeshFilter>();
        mf.mesh = bakedMesh;

        MeshRenderer mr = meshObject.AddComponent<MeshRenderer>();
        mr.materials = skinnedMeshRenderer.materials;

        skinnedMeshRenderer.gameObject.SetActive(false);

        _bakedMesh = mr;
    }

    private void AddRayfireComponents()
    {
        RayfireRigid rigid = _bakedMesh.gameObject.AddComponent<RayfireRigid>();
        rigid.simulationType = SimType.Inactive;

        rigid.physics.mt = MaterialType.Glass;

        rigid.damage.max = 1;
        rigid.damage.en = true;

        rigid.demolitionType = DemolitionType.AwakePrefragment;
        rigid.meshDemolition.cls = true;


        rigid.physics.ct = RFColliderType.Box;

        rigid.Initialize();

        DOVirtual.DelayedCall(1, () =>
        {
            //rigid.ApplyDamage(100, _damagePoint.transform.position, 1);
        });
    }
}
