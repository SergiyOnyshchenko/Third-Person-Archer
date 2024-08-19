using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class BowViewController : System, IBowView
    {
        [SerializeField] private BowView[] _bowView;
        private BowView _currentBowView;
        public GameObject Model => _currentBowView.Model;
        public BowSpring BowSpring => _currentBowView.BowSpring;
        public GameObject Arrow => _currentBowView.Arrow;

        private void Awake()
        {
            SetView(1);
        }

        public void SetView(int index)
        {
            if (index >= _bowView.Length)
                return;

            foreach (var view in _bowView)
                view.Model.SetActive(false);

            _currentBowView = _bowView[index];
            _currentBowView.Model.SetActive(true);
        }

    }
}