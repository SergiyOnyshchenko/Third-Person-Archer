using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class PlayerSkinController : System
    {
        [SerializeField] private EquippedPlayerSkinData _data;
        [SerializeField] private PlayerSkin[] _playerSkins;
        [SerializeField] private Actor.Animator _animator;
        [SerializeField] private bool _destroyUnusedSkins = true;
        private PlayerSkin _currentSkin;
        public PlayerSkin CurrentSkin { get => _currentSkin; }
        public UnityEvent OnSkinChanged = new UnityEvent();

        private void Awake()
        {
            SetSkin(_data.SkinData);
        }

        private void OnEnable()
        {
            _data.OnEquipped += SetSkin;
        }

        private void OnDisable()
        {
            _data.OnEquipped -= SetSkin;
        }

        public void SetSkin(PlayerSkinData skinData)
        {
            foreach (var skin in _playerSkins)
                skin.ShowView(false);

            foreach (var skin in _playerSkins)
            {
                if (skin.Index == skinData.Index)
                {
                    skin.ShowView(true);
                    _animator.SetAnimator(skin.Animator);

                    _currentSkin = skin;
                }
                else
                {
                    foreach (var model in skin.Models)
                    {
                        if (_destroyUnusedSkins)
                            Destroy(model);
                        else
                            model.gameObject.SetActive(false);
                    }     
                }
            }

            if (_destroyUnusedSkins)
                Destroy(this);

            OnSkinChanged?.Invoke();
        }
    }
}