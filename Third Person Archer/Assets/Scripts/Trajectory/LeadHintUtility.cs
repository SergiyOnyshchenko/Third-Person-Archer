using UnityEngine;

public static class LeadHintUtility
{
    public static bool TryFindLeadPoint(in TrajectoryPrediction prediction, Vector3 targetPos, Vector3 targetVelocity,
        out Vector3 leadPoint, float maxSearchTime)
    {
        leadPoint = default;
        if (prediction.PointCount <= 1) return false;

        float bestDistSq = float.MaxValue;
        int bestIndex = -1;

        // Approximate per-segment time by uniform dt
        float dt = (prediction.TimeAtHit > 0f ? prediction.TimeAtHit : maxSearchTime) / Mathf.Max(1, prediction.PointCount - 1);

        for (int i = 0; i < prediction.PointCount; i++)
        {
            float t = i * dt;
            Vector3 expected = targetPos + targetVelocity * t;
            Vector3 arc = prediction.Points[i];
            float d2 = (arc - expected).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
        {
            leadPoint = prediction.Points[bestIndex];
            return true;
        }
        return false;
    }
}