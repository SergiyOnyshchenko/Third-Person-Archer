

using Actor.Properties;

namespace Actor
{
    public class CrouchInput : Input, IActorIniter
    {
        private Animator _animator;
        private Speed _speed;
        private const string _crouchName = "Crouch";

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetSystem(out _animator)){}
            if(actor.TryGetProperty(out _speed)){}
        }

        public void StartCrouch()
        {
            _animator.SetBool(_crouchName, true);
            _speed.SetValue(2.25f);
        }

        public void FinishCrouch()
        {
            _animator.SetBool(_crouchName, false);
             _speed.ResetValue();
        }
    }
}