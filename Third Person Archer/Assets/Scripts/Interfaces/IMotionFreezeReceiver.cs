public interface IMotionFreezeReceiver
{
    public delegate void FreezeStateChange(bool isFreezed);
    public FreezeStateChange FreezeAction { get; set; }

    public void SendFreezeState(bool isFreezed)
    {
        FreezeAction(isFreezed);
    }
}
