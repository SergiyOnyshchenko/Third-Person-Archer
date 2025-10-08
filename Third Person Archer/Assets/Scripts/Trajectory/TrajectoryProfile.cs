using UnityEngine;

[CreateAssetMenu(fileName = "TrajectoryProfile", menuName = "Combat/Trajectory Profile")]
public sealed class TrajectoryProfile : ScriptableObject
{
    [Header("Launch")]
    [Tooltip("Max launch speed at full pull (units/sec).")]
    public float MaxSpeed = 60f;

    [Tooltip("Maps normalized pull [0..1] to speed factor [0..1].")]
    public AnimationCurve PullToSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Stylized Gravity")]
    [Tooltip("Downward acceleration (units/sec^2). Use positive value. Applied along -Y.")]
    public float Gravity = 30f;

    [Header("Prediction")]
    [Tooltip("Max simulated time horizon for preview (seconds).")]
    public float MaxPreviewTime = 2.5f;

    [Tooltip("Maximum number of segments (line samples).")]
    public int MaxPreviewSegments = 48;

    [Tooltip("Collision mask used for prediction linecasts.")]
    public LayerMask CollisionMask;

    [Header("Visual")]
    [Tooltip("Optional impact marker to place at predicted hit.")]
    public GameObject ImpactMarkerPrefab;
}