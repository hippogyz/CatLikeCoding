using CustomRP.CustomShadow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        CameraRenderer renderer = new CameraRenderer();

        bool use_dynamic_batching;
        bool use_gpu_instancing;

        ShadowSettings shadow_settings;

        public CustomRenderPipeline(
            bool useSPRBatching, bool useDynamicBatching, bool useGPUInstancing,
            ShadowSettings shadowSettings
            )
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = useSPRBatching;

            use_dynamic_batching = useDynamicBatching;
            use_gpu_instancing = useGPUInstancing;

            shadow_settings = shadowSettings;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                renderer.Render(context, camera, 
                    use_dynamic_batching, use_gpu_instancing,
                    shadow_settings);
            }
        }
    }
}
