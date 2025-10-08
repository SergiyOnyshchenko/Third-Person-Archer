using UnityEngine;

public interface ITrajectoryPredictor
{
    TrajectoryPrediction Predict(in TrajectoryPredictParams p);
    bool TryHitEnemy(in TrajectoryPredictParams p, LayerMask enemyLayer, out GameObject hitEnemy, out Vector3 hitPosition);
}

public struct TrajectoryPredictParams
{
    public Vector3 Origin;
    public Vector3 InitialVelocity;   // direction * speed
    public float Gravity;             // positive scalar (applied as -Y)
    public float MaxTime;
    public int MaxSegments;
    public LayerMask CollisionMask;
}

public struct TrajectoryPrediction
{
    public Vector3[] Points;          // allocated once, reused
    public int PointCount;
    public bool Hit;
    public RaycastHit HitInfo;
    public float TimeAtHit;
}
