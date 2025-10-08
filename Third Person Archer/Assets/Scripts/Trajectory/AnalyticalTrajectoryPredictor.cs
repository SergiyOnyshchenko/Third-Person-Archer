using UnityEngine;

public sealed class AnalyticalTrajectoryPredictor : ITrajectoryPredictor
{
    // Reusable buffer to avoid per-frame GC
    private readonly Vector3[] _pointsBuffer;

    public AnalyticalTrajectoryPredictor(int maxSegmentsCapacity = 128)
    {
        _pointsBuffer = new Vector3[Mathf.Max(8, maxSegmentsCapacity + 1)];
    }

    public TrajectoryPrediction Predict(in TrajectoryPredictParams p)
    {
        // Clamp & derive step
        int segments = Mathf.Clamp(p.MaxSegments, 2, _pointsBuffer.Length - 1);
        float dt = p.MaxTime / segments;
        Vector3 g = new Vector3(0f, -Mathf.Abs(p.Gravity), 0f);

        Vector3 prevPos = p.Origin;
        _pointsBuffer[0] = prevPos;

        float t = 0f;
        int count = 1;
        RaycastHit hit = new RaycastHit();
        bool hasHit = false;
        float timeAtHit = 0f;

        for (int i = 1; i <= segments; i++)
        {
            t += dt;
            Vector3 pos = Ballistics.PointAtTime(p.Origin, p.InitialVelocity, g, t);

            // Linecast from prev to current to catch thin colliders
            if (Physics.Linecast(prevPos, pos, out hit, p.CollisionMask, QueryTriggerInteraction.Ignore))
            {
                hasHit = true;
                timeAtHit = t;
                // Clamp the point to the exact hit
                pos = hit.point;
                _pointsBuffer[count++] = pos;
                break;
            }

            _pointsBuffer[count++] = pos;
            prevPos = pos;
        }

        return new TrajectoryPrediction
        {
            Points = _pointsBuffer,
            PointCount = count,
            Hit = hasHit,
            HitInfo = hasHit ? hit : default,
            TimeAtHit = hasHit ? timeAtHit : t
        };
    }

    public bool TryHitEnemy(in TrajectoryPredictParams p, LayerMask enemyLayer, out GameObject hitEnemy, out Vector3 hitPosition)
    {
        hitEnemy = null;
        hitPosition = Vector3.zero;

        int segments = Mathf.Clamp(p.MaxSegments, 2, _pointsBuffer.Length - 1);
        float dt = p.MaxTime / segments;
        Vector3 g = new Vector3(0f, -Mathf.Abs(p.Gravity), 0f);

        Vector3 prevPos = p.Origin;
        float t = 0f;

        for (int i = 1; i <= segments; i++)
        {
            t += dt;
            Vector3 pos = Ballistics.PointAtTime(p.Origin, p.InitialVelocity, g, t);

            // First, check if it hits an enemy
            if (Physics.Linecast(prevPos, pos, out RaycastHit enemyHit, enemyLayer, QueryTriggerInteraction.Ignore))
            {
                hitEnemy = enemyHit.collider.gameObject;
                hitPosition = enemyHit.point;
                return true;
            }

            // Optional: stop early if it hits environment before enemy
            if (Physics.Linecast(prevPos, pos, p.CollisionMask, QueryTriggerInteraction.Ignore))
                return false;

            prevPos = pos;
        }

        return false;
    }
}

internal static class Ballistics
{
    // p(t) = p0 + v0 t + 0.5 g t^2
    public static Vector3 PointAtTime(Vector3 origin, Vector3 v0, Vector3 g, float t)
    {
        return origin + v0 * t + 0.5f * g * (t * t);
    }
}