using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class MeshBall : MonoBehaviour
    {
        static readonly int color_id = Shader.PropertyToID("_BaseColor");

        public Mesh mesh;
        public Material material;

        Matrix4x4[] matrices = new Matrix4x4[1023];
        Matrix4x4[] world_matrices = new Matrix4x4[1023];
        Vector4[] colors = new Vector4[1023];

        MaterialPropertyBlock block;

        private void Awake()
        {
            for(int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = Matrix4x4.TRS(
                    Random.onUnitSphere * 10f,
                    Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                    Vector3.one * (1 + 0.3f * (Random.value - 0.5f))
                    );

                colors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1));
            }

            UpdateWorldCoord();
        }

        private void Update()
        {
            if(block == null)
            {
                block = new MaterialPropertyBlock();
                block.SetVectorArray(color_id, colors);
            }

            if(transform.hasChanged)
                UpdateWorldCoord();

            Graphics.DrawMeshInstanced(mesh, 0, material, world_matrices, 1023, block);
        }

        private void UpdateWorldCoord()
        {
            for (int i = 0; i < matrices.Length; ++i)
                world_matrices[i] = transform.localToWorldMatrix * matrices[i];
        }
    }

}
