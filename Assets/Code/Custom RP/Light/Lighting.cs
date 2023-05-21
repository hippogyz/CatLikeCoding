using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Light
{
    public class Lighting
    {
        CullingResults culling_results;

        const string BUFFER_NAME = "Lighting";
        CommandBuffer cmd = new CommandBuffer { name = BUFFER_NAME };

        // static readonly int DIR_LIGHT_COLOR_ID = Shader.PropertyToID("_DirectionalLightColor");
        // static readonly int DIR_LIGHT_DIRECTION_ID = Shader.PropertyToID("_DirectionalLightDirection");

        #region Directional Lights
        const int MAX_DIR_LIGHT_COUNT = 4;

        static readonly int DIR_LIGHT_COUNT_ID = Shader.PropertyToID("_DirectionalLightCount");
        static readonly int DIR_LIGHT_COLORS_ID = Shader.PropertyToID("_DirectionalLightColors");
        static readonly int DIR_LIGHT_DIRECTIONS_ID = Shader.PropertyToID("_DirectionalLightDirections");

        static Vector4[] dir_light_colors = new Vector4[MAX_DIR_LIGHT_COUNT];
        static Vector4[] dir_light_directions = new Vector4[MAX_DIR_LIGHT_COUNT];
        #endregion

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
        {
            culling_results = cullingResults;

            cmd.BeginSample(BUFFER_NAME);

            SetupLights();

            cmd.EndSample(BUFFER_NAME);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        private void SetupLights()
        {
            var visible_lights = culling_results.visibleLights;

            int dir_light_count = 0;
            for (int i = 0; i < visible_lights.Length; ++i)
            {
                var light = visible_lights[i];
                
                // Directional
                if (light.lightType == LightType.Directional)
                {
                    if (dir_light_count < MAX_DIR_LIGHT_COUNT)
                    {
                        SetupDirectionalLight(dir_light_count, ref light);
                        dir_light_count++;
                    }
                }

            }

            // Directional
            cmd.SetGlobalInt(DIR_LIGHT_COUNT_ID, dir_light_count);
            cmd.SetGlobalVectorArray(DIR_LIGHT_COLORS_ID, dir_light_colors);
            cmd.SetGlobalVectorArray(DIR_LIGHT_DIRECTIONS_ID, dir_light_directions);
        }

        private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            dir_light_colors[index] = visibleLight.finalColor;
            dir_light_directions[index] = -visibleLight.localToWorldMatrix.GetColumn(2); // z-axis
        }

        //private void SetupDirectionalLight(CommandBuffer command)
        //{
        //    // Main Directional Light
        //    var light = RenderSettings.sun;
        //    var color = light.color.linear * light.intensity;
        //    var direction = -light.transform.forward;

        //    command.SetGlobalVector(DIR_LIGHT_COLOR_ID, color);
        //    command.SetGlobalVector(DIR_LIGHT_DIRECTION_ID, direction);
        //}
    }

}
