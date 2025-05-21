using UnityEngine;

public class AvatarRetargeter : MonoBehaviour
{
    [SerializeField] private Animator sourceAnimator; // Source model with animation
    [SerializeField] private Animator targetAnimator; // Target model to retarget animation to

    private HumanPoseHandler sourcePoseHandler; // Handles source muscle data
    private HumanPoseHandler targetPoseHandler; // Handles target muscle data
    private HumanPose humanPose; // Stores pose data

    void Start()
    {
        if (sourceAnimator == null || targetAnimator == null)
        {
            Debug.LogError("Source or Target Animator not assigned!");
            return;
        }

        if (sourceAnimator.avatar == null || targetAnimator.avatar == null)
        {
            Debug.LogError("Source or Target Avatar not assigned!");
            return;
        }

        // Initialize pose handlers
        sourcePoseHandler = new HumanPoseHandler(sourceAnimator.avatar, sourceAnimator.transform);
        targetPoseHandler = new HumanPoseHandler(targetAnimator.avatar, targetAnimator.transform);
        humanPose = new HumanPose();
    }

    void LateUpdate()
    {
        if (sourceAnimator == null || targetAnimator == null)
            return;

        // Get the source pose
        sourcePoseHandler.GetHumanPose(ref humanPose);

        // Apply the pose to the target
        targetPoseHandler.SetHumanPose(ref humanPose);
    }

    void OnDestroy()
    {
        // Clean up pose handlers
        sourcePoseHandler?.Dispose();
        targetPoseHandler?.Dispose();
    }
}