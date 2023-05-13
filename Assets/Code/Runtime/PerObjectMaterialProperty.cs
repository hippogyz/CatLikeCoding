using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class PerObjectMaterialProperty : MonoBehaviour
    {
        public Color color;

        static MaterialPropertyBlock block;
        static readonly int color_id = Shader.PropertyToID("_BaseColor");


        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if(block == null)
                block = new MaterialPropertyBlock();

            block.SetColor(color_id, color);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }

    }
}
