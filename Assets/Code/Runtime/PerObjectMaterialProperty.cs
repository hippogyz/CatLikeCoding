using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class PerObjectMaterialProperty : MonoBehaviour
    {
        public Color color;
        [Range(0, 1)] public float alphaCutoff = 0;

        static MaterialPropertyBlock block;
        static readonly int color_id = Shader.PropertyToID("_BaseColor");
        static readonly int cutoff_id = Shader.PropertyToID("_Cutoff");

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

            GetComponent<Renderer>().SetPropertyBlock(block);
        }

    }
}
