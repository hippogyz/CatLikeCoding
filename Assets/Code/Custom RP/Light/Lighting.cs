using CustomRP.CustomShadow;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.CustomLight
{
    public class Lighting
    {
        CullingResults culling_results;
        Shadows shadows = new Shadows();

        #region CommandBuffer
        const string BUFFER_NAME = "Lighting";
        CommandBuffer cmd = new CommandBuffer { name = BUFFER_NAME };
        #endregion

        #region Directional Lights
        const int MAX_DIR_LIGHT_COUNT = 4;

        static readonly int DIR_LIGHT_COUNT_ID = Shader.PropertyToID("_DirectionalLightCount");
        static readonly int DIR_LIGHT_COLORS_ID = Shader.PropertyToID("_DirectionalLightColors");
        static readonly int DIR_LIGHT_DIRECTIONS_ID = Shader.PropertyToID("_DirectionalLightDirections");
        static readonly int DIR_LIGHT_SHADOW_DATA_ID = Shader.PropertyToID("_DirectionalLightShadowData");

        static Vector4[] dir_light_colors = new Vector4[MAX_DIR_LIGHT_COUNT];
        static Vector4[] dir_light_directions = new Vector4[MAX_DIR_LIGHT_COUNT];
        static Vector4[] dir_light_shadow_data = new Vector4[MAX_DIR_LIGHT_COUNT];


        // static readonly int DIR_LIGHT_COLOR_ID = Shader.PropertyToID("_DirectionalLightColor");
        // static readonly int DIR_LIGHT_DIRECTION_ID = Shader.PropertyToID("_DirectionalLightDirection");

        #endregion

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, 
            ShadowSettings shadowSettings)
        {
            culling_results = cullingResults;

            cmd.BeginSample(BUFFER_NAME);

            shadows.Setup(context, cullingResults, shadowSettings); // init shadow
            SetupLights(); // 包括传递光照信息给阴影
            shadows.Render();

            cmd.EndSample(BUFFER_NAME);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }


        public void Cleanup()
        {
            shadows.Cleanup();
        }

        private void SetupLights()
        {
            var visible_lights = culling_results.visibleLights;

            var dir_light_count = 0;
            for (int i = 0; i < visible_lights.Length; ++i)
            {
                var light = visible_lights[i];
                
                // Directional
                if (light.lightType == LightType.Directional)
                {
                    if (dir_light_count < MAX_DIR_LIGHT_COUNT)
                    {
                        SetupDirectionalLight(
                            indexInDir: dir_light_count, 
                            indexInCull: i, 
                            ref light);
                        dir_light_count++;
                    }
                }
            }

            // Directional
            cmd.SetGlobalInt(DIR_LIGHT_COUNT_ID, dir_light_count);
            cmd.SetGlobalVectorArray(DIR_LIGHT_COLORS_ID, dir_light_colors);
            cmd.SetGlobalVectorArray(DIR_LIGHT_DIRECTIONS_ID, dir_light_directions);
            cmd.SetGlobalVectorArray(DIR_LIGHT_SHADOW_DATA_ID, dir_light_shadow_data);
        }

        private void SetupDirectionalLight(int indexInDir, int indexInCull, ref VisibleLight visibleLight)
        {
            dir_light_colors[indexInDir] = visibleLight.finalColor;
            dir_light_directions[indexInDir] = -visibleLight.localToWorldMatrix.GetColumn(2); // z-axis

            // shadows
            dir_light_shadow_data[indexInDir] = shadows.ReserveDirectionalShadows(visibleLight.light, indexInCull);
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
