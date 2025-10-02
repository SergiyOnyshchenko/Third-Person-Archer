using UnityEngine;

public static class VisibilityUtility
{
    /// <summary>
    /// Checks if a world point is inside the camera frustum (ignores occlusion).
    /// </summary>
    public static bool IsPointInCameraFrustum(Camera camera, Vector3 worldPoint)
    {
        if (camera == null) return false;

        Vector3 vp = camera.WorldToViewportPoint(worldPoint);
        if (vp.z < 0f) return false; // Behind camera

        return (vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f);
    }

    /// <summary>
    /// Returns true if the point is occluded by an object in occluderLayers
    /// from the camera’s position.
    /// </summary>
    public static bool IsOccludedFromCamera(
        Camera camera,
        Vector3 worldPoint,
        LayerMask losRaycastMask,
        LayerMask occluderLayers)
    {
        if (camera == null) return false;

        Vector3 eye = camera.transform.position;
        Vector3 dir = worldPoint - eye;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return false;

        if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, dist, losRaycastMask, QueryTriggerInteraction.Ignore))
        {
            if (((1 << hit.collider.gameObject.layer) & occluderLayers) != 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if point is considered "hidden" from camera
    /// (either outside frustum OR occluded by obstacle).
    /// </summary>
    public static bool IsHiddenFromCamera(
        Camera camera,
        Vector3 worldPoint,
        LayerMask losRaycastMask,
        LayerMask occluderLayers)
    {
        bool inView = IsPointInCameraFrustum(camera, worldPoint);
        bool occluded = IsOccludedFromCamera(camera, worldPoint, losRaycastMask, occluderLayers);

        return !inView || occluded;
    }
}