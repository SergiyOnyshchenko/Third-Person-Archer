using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class ModelView : System
    {
        private Outline _outline;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            Highlight(false);
        }

        public void Highlight(bool value)
        {
            if (_outline == null)
                return;

            if (value)
                _outline.OutlineWidth = 0f;
            else
                _outline.OutlineWidth = 0;
        }
    }
}