using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class ModelView : System
    {
        private SkinnedMeshRenderer[] _renderers;
        private Outline _outline;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();    
            _outline = GetComponent<Outline>();
            Highlight(false);
        }

        public void Highlight(bool value)
        {
            if (_outline == null)
                return;

            if (value)
                _outline.OutlineWidth = 1f;
            else
                _outline.OutlineWidth = 0;
        }

        public void SetEmmissionColor(Color color)
        {
            foreach (var renderer in _renderers)
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    renderer.materials[i].SetColor("_EmissionColor", color);
                }
            }
        }
    }
}