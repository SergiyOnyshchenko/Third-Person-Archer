using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Keeps an overlay camera's lens identical to the main camera's, so what it draws (the x-ray skeleton)
/// lines up with the scene. Copied right before rendering: Cinemachine sets the main camera's lens in LateUpdate,
/// and the kill cam changes the FOV every frame, so copying in Update left the overlay one frame behind.
/// </summary>
public class OverlayCameraHelper : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _overlayCamera;

    private void OnEnable()
    {
        RenderPipelineManager.beginContextRendering += OnBeginRendering;
        Sync();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginContextRendering -= OnBeginRendering;
    }

    private void OnBeginRendering(ScriptableRenderContext context, System.Collections.Generic.List<Camera> cameras)
    {
        Sync();
    }

    private void Sync()
    {
        if (_mainCamera == null || _overlayCamera == null)
            return;

        _overlayCamera.fieldOfView = _mainCamera.fieldOfView;
        _overlayCamera.nearClipPlane = _mainCamera.nearClipPlane;
    }
}
