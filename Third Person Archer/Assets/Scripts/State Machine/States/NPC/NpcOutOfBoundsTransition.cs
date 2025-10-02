using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.AI;

public class NpcOutOfBoundsTransition : StateTransition, IActorIniter
{
    [Header("Visibility Settings")]
    [Tooltip("Layers considered as occluders (e.g., terrain, walls, props).")]
    [SerializeField] private LayerMask _occluderLayers = ~0;

    [Tooltip("Layers used in the line-of-sight raycast (usually matches occluders; exclude NPC layer).")]
    [SerializeField] private LayerMask _losRaycastMask = ~0;

    [Tooltip("Extra offset up from ground/feet for visibility checks (e.g., eye height).")]
    [SerializeField] private float _heightOffset = 1.6f;

    [Header("Check Interval (Seconds)")]
    [SerializeField] private Vector2 _intervalRange = new Vector2(0.5f, 2f);
    private Camera _camera;
    private Coroutine _loop;

    public void InitActor(ActorController actor)
    {
        _camera = Camera.main;
    }

    public override void Enter()
    {
        base.Enter();

        if (_loop == null)
            _loop = StartCoroutine(CheckLoop());
    }

    public override void Exit()
    {
        StopAllRunningCoroutines();

        base.Exit();
    }

    private void StopAllRunningCoroutines()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
    }

        private IEnumerator CheckLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(_intervalRange.x, _intervalRange.y);
            yield return new WaitForSeconds(wait);

            if (!IsVisible())
            {
                OnBecameInvisibleToCamera();
            }
        }
    }

    private bool IsVisible()
    {
        // Add Y offset (eye height)
        Vector3 checkPoint = transform.position + Vector3.up * _heightOffset;

        bool inFrustum = VisibilityUtility.IsPointInCameraFrustum(_camera, checkPoint);
        bool occluded  = VisibilityUtility.IsOccludedFromCamera(_camera, checkPoint, _losRaycastMask, _occluderLayers);

        return inFrustum && !occluded;
    }

    protected virtual void OnBecameInvisibleToCamera()
    {
        DoTransition();
    }
}
