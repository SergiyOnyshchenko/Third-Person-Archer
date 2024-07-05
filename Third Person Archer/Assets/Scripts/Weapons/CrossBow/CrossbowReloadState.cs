using Actor;
using DG.Tweening;

public class CrossbowReloadState : ProcessState, IActorIniter
{
    private CrossbowController _crossbowController;

    public override void Enter()
    {
        base.Enter();
        Reload();
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out CrossbowController crossbow))
            _crossbowController = crossbow;
    }

    private void LateUpdate()
    {
        _crossbowController.UpdateHands();
    }

    private void Reload()
    {
        _crossbowController.Reload();
        DOVirtual.DelayedCall(0.5f, FinishProcess);
    }
}
