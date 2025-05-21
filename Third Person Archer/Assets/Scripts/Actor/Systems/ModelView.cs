using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Actor
{
    public class ModelView : System
    {
        [SerializeField] private SkinnedMeshRenderer[] _renderers;
        private Material [][] _normalMaterials;
        private Outline _outline;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            _normalMaterials = new Material[_renderers.Length][];

            for (int i = 0; i < _renderers.Length; i++)
            {
                _normalMaterials[i] = new Material[_renderers[i].materials.Length];

                for (int j = 0; j < _normalMaterials[i].Length; j++)
                {
                    _normalMaterials[i][j] = _renderers[i].materials[j];
                }
            }

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

        public void SetMaterial(Material material)
        {
            Debug.Log(gameObject.name + " Set Material");

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = false;
            }

            /*
            for (int i = 0; i < _renderers.Length; i++)
            {
                List<Material> materials = new List<Material>(_renderers[i].materials.Length);

                for (int j = 0; j < _normalMaterials[i].Length; j++)
                {

                }

                _renderers[i].SetMaterials(materials);
            }
            */
        }

        public void ResetMaterials()
        {
            for (int i = 0; i < _normalMaterials.Length; i++)
            {
                for (int j = 0; j < _normalMaterials[i].Length; j++)
                {
                    _renderers[i].materials[j] = _normalMaterials[i][j];
                }
            }
        }
    }
}