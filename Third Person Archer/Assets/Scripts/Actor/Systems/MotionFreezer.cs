using UnityEngine;

namespace Actor
{
    public class MotionFreezer : System, IActorIniter
    {
        [SerializeField] private Animator _animator;

        private IMotionFreezeReceiver[] _freezeRecievers;
        public void InitActor(ActorController actor)
        {
            _freezeRecievers = actor.GetComponentsInChildren<IMotionFreezeReceiver>();
            SubscribeDamageRecievers();
        }

        private void SubscribeDamageRecievers()
        {
            foreach (IMotionFreezeReceiver reciever in _freezeRecievers)
            {
                reciever.FreezeAction += FreezeMotionHandler;
            }
        }

        private void UnsubscribeDamageRecievers()
        {
            foreach (IMotionFreezeReceiver reciever in _freezeRecievers)
            {
                reciever.FreezeAction -= FreezeMotionHandler;
            }
        }

        private void FreezeMotionHandler(bool isFreezed)
        {

        }
    }
}