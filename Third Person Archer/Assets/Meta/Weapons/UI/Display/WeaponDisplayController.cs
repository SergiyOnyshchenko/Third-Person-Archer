using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Meta.Weapons.UI.Selection
{
    /// <summary>
    /// Spawns a weapon prefab and controls idle rotation & DOTween-based posing for Selection/Upgrade UI.
    /// </summary>
    public class WeaponDisplayController : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform modelParent;

        [Header("Idle Rotation")]
        [SerializeField] private float rotationSpeedDegreesPerSecond = 15f;

        [Header("Tween Defaults")]
        [SerializeField] private bool tweenIgnoresTimeScale = true;   // UI normally runs while game is paused
        [SerializeField] private Ease moveEase = Ease.OutCubic;
        [SerializeField] private Ease rotateEase = Ease.OutCubic;

        [Header("Layering (optional)")]
        [SerializeField] private bool applyOverlayLayer = true;
        [SerializeField] private string overlayLayerName = "Overlay";

        // ---- Runtime state ----
        private IWeaponPrefabProvider prefabProvider;
        private GameObject currentInstance;
        private bool rotate;

        private DG.Tweening.Sequence poseSequence;    // joined move+rotate tween
        private Tween idleRotateTween;    // not used (we keep simple Update rotate), left for future

        public void Initialize(IWeaponPrefabProvider provider) => prefabProvider = provider;

        public void LoadWeaponModel(string weaponId)
        {
            // Kill tweens and unload previous
            KillPoseTweens();
            UnloadCurrent();

            if (prefabProvider == null)
            {
                Debug.LogWarning("WeaponDisplayController: Prefab provider not set.");
                return;
            }

            var prefab = prefabProvider.GetPrefab(weaponId);
            if (prefab == null)
            {
                Debug.LogWarning($"WeaponDisplayController: No prefab for '{weaponId}'.");
                return;
            }

            var parent = modelParent != null ? modelParent : transform;
            currentInstance = Instantiate(prefab, parent);
            currentInstance.transform.localPosition = Vector3.zero;
            currentInstance.transform.localRotation = Quaternion.identity;

            if (applyOverlayLayer && !string.IsNullOrEmpty(overlayLayerName))
            {
                int overlayLayer = LayerMask.NameToLayer(overlayLayerName);
                if (overlayLayer >= 0) SetLayerRecursively(currentInstance.transform, overlayLayer);
            }
        }

        public void UnloadCurrent()
        {
            if (currentInstance != null)
            {
                KillPoseTweens();
                Destroy(currentInstance);
                currentInstance = null;
            }
        }

        public void SetRotationEnabled(bool enabled) => rotate = enabled;
        public void SetRotationSpeed(float degPerSec) => rotationSpeedDegreesPerSecond = degPerSec;

        private void Update()
        {
            if (rotate && currentInstance != null)
            {
                currentInstance.transform.Rotate(Vector3.up, rotationSpeedDegreesPerSecond * Time.deltaTime, Space.World);
            }
        }

        // ========== POSE API (used by presenters) ==========

        /// <summary>Hard snap the model to a base local position/rotation.</summary>
        public void ApplyBasePose(Vector3 localPosition, Vector3 localEulerAngles)
        {
            if (currentInstance == null) return;
            KillPoseTweens();

            var tr = currentInstance.transform;
            tr.localPosition = localPosition;
            tr.localRotation = Quaternion.Euler(localEulerAngles);
        }

        /// <summary>Smoothly move/rotate the model to a focus pose.</summary>
        public void SmoothFocusPose(Vector3 localPosition, Vector3 localEulerAngles, float duration = 0.25f)
        {
            if (currentInstance == null) return;
            KillPoseTweens();

            var tr = currentInstance.transform;
            
            // Validate duration to prevent issues
            duration = Mathf.Max(0.001f, duration);
            
            poseSequence = DOTween.Sequence();

            var move = tr.DOLocalMove(localPosition, duration)
                        .SetEase(moveEase)
                        .SetUpdate(tweenIgnoresTimeScale);
                        
            var rotate = tr.DOLocalRotate(localEulerAngles, duration, RotateMode.Fast)
                          .SetEase(rotateEase)
                          .SetUpdate(tweenIgnoresTimeScale);

            poseSequence.Join(move).Join(rotate)
                        .SetAutoKill(true)
                        .Play();
        }

        /// <summary>Alias kept for older call sites.</summary>
        public void SmoothToLocalPose(Vector3 localPosition, Vector3 localEulerAngles, float duration = 0.25f)
            => SmoothFocusPose(localPosition, localEulerAngles, duration);

        /// <summary>Small attention wiggle when no explicit pose exists.</summary>
        public void NudgeForAttention(Vector3 eulerDelta, float duration = 0.2f)
        {
            if (currentInstance == null) return;
            KillPoseTweens();

            var tr = currentInstance.transform;
            var start = tr.localEulerAngles;
            var target = start + eulerDelta;

            // Validate duration to prevent issues
            duration = Mathf.Max(0.001f, duration);
            float halfDuration = duration * 0.5f;

            poseSequence = DOTween.Sequence()
                .SetUpdate(tweenIgnoresTimeScale)
                .SetAutoKill(true);
                
            poseSequence.Append(tr.DOLocalRotate(target, halfDuration, RotateMode.Fast)
                                  .SetEase(Ease.OutSine)
                                  .SetUpdate(tweenIgnoresTimeScale));
            poseSequence.Append(tr.DOLocalRotate(start, halfDuration, RotateMode.Fast)
                                  .SetEase(Ease.InSine)
                                  .SetUpdate(tweenIgnoresTimeScale));
            poseSequence.Play();
        }

        // ========== Helpers ==========

        private void KillPoseTweens()
        {
            if (poseSequence != null && poseSequence.IsActive())
            {
                poseSequence.Kill();
                poseSequence = null;
            }

            if (idleRotateTween != null && idleRotateTween.IsActive())
            {
                idleRotateTween.Kill();
                idleRotateTween = null;
            }
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            if (root == null) return;
            
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursively(root.GetChild(i), layer);
        }

        private void OnDestroy() => KillPoseTweens();
        private void OnDisable() => KillPoseTweens();
    }
}