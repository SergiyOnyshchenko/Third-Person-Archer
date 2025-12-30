public interface IMissionCompletionService
{
    MissionCompleteResult Complete(MissionContext ctx, MissionOutcome outcome);
}