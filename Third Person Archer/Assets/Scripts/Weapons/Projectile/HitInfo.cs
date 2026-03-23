namespace Actor
{
    public class HitInfo
    {
        public bool HitTarget { get; }
        public bool TargetIsActor { get; }
        public ActorController Actor { get; }

        public HitInfo(bool hitTarget, ActorController actor = null)
        {
            HitTarget = hitTarget;
            Actor = actor;
            TargetIsActor = actor != null;
        }
    }
}