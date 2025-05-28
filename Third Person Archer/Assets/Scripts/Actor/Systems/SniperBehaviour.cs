using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class SniperBehaviour : System, IActorIniter
    {
        [SerializeField] private bool _isActive;
        [Space]
        [SerializeField] private GameObject _sniperUI;

        private ZoomFovMultiplier _zoomFovMult;
        private float _zoomMult = 0.3f;

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetProperty(out _zoomFovMult)) { }

            SetSniperMode(_isActive);
        }

        public void SetSniperMode(bool value)
        {
            _isActive = value;
            _sniperUI.SetActive(value);

            if(value)
                _zoomFovMult.SetValue(_zoomMult);
            else
                _zoomFovMult.SetValue(1);
        }
    }
}