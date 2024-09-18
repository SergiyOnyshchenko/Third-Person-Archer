using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class SpearViewController : System
    {
        [SerializeField] private GameObject[] _spearView;
        private GameObject _currentSpearView;

        public void SetView(int index)
        {
            if (index >= _spearView.Length)
                return;

            foreach (var view in _spearView)
                view.SetActive(false);

            _currentSpearView = _spearView[index];
            _currentSpearView.SetActive(true);
        }
    }
}