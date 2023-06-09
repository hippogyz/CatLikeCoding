using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime
{
    public class MeshBall : MonoBehaviour
    {
        static readonly int color_id = Shader.PropertyToID("_BaseColor");
        static readonly int metallic_id = Shader.PropertyToID("_Metallic");
        static readonly int smoothness_id = Shader.PropertyToID("_Smoothness");

        public Mesh mesh;
        public Material material;
        public LightProbeProxyVolume lightProbeVolume;

        const int SIZE = 1023;

        Matrix4x4[] matrices = new Matrix4x4[SIZE];
        Matrix4x4[] world_matrices = new Matrix4x4[SIZE];
        Vector4[] colors = new Vector4[SIZE];
        float[] metallics = new float[SIZE];
        float[] smoothness = new float[SIZE];

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

                // colors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1));
                colors[i] = new Vector4(1.0f, 1.0f, 1.0f, Random.Range(0.5f, 1));
                metallics[i] = Random.value;
                smoothness[i] = Random.value * 0.9f + 0.05f;
            }

            UpdateWorldCoord();
        }

        private void Update()
        {
            if(block == null)
            {
                block = new MaterialPropertyBlock();
                block.SetVectorArray(color_id, colors);
                block.SetFloatArray(metallic_id, metallics);
                block.SetFloatArray(smoothness_id, smoothness);

                var positions = new Vector3[SIZE];
                for (int i = 0; i < SIZE; ++i)
                    positions[i] = world_matrices[i].GetColumn(3);

                var light_probes = new SphericalHarmonicsL2[SIZE];
                LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, light_probes, null);
                block.CopySHCoefficientArraysFrom(light_probes);
            }

            if(transform.hasChanged)
                UpdateWorldCoord();

            var light_probe_usage = lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided;

            Graphics.DrawMeshInstanced(mesh, 0, material, world_matrices, SIZE, block,
                ShadowCastingMode.On, receiveShadows: true, layer: 0, camera: null, light_probe_usage, lightProbeVolume
                );
        }

        private void UpdateWorldCoord()
        {
            for (int i = 0; i < matrices.Length; ++i)
                world_matrices[i] = transform.localToWorldMatrix * matrices[i];
        }
    }

}
