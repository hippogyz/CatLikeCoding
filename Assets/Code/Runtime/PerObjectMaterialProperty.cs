using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class PerObjectMaterialProperty : MonoBehaviour
    {
        public Color color;
        [Range(0, 1)] public float alphaCutoff = 0;
        [Range(0, 1)] public float metallic = 0;
        [Range(0, 1)] public float smoothness = 0.5f;

        static MaterialPropertyBlock block;
        static readonly int color_id = Shader.PropertyToID("_BaseColor");
        static readonly int cutoff_id = Shader.PropertyToID("_Cutoff");
        static readonly int metallic_id = Shader.PropertyToID("_Metallic");
        static readonly int smoothness_id = Shader.PropertyToID("_Smoothness");

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if(block == null)
                block = new MaterialPropertyBlock();

            block.SetColor(color_id, color);
            block.SetFloat(cutoff_id, alphaCutoff);
            block.SetFloat(metallic_id, metallic);
            block.SetFloat(smoothness_id, smoothness);

            GetComponent<Renderer>().SetPropertyBlock(block);
        }

    }
}
