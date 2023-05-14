using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class CameraRenderer
    {
        ScriptableRenderContext context;
        Camera camera;

        const string cmd_name = "Render Camera";
        CommandBuffer cmd = new CommandBuffer { name = cmd_name };

        CullingResults culling_results;

        static ShaderTagId unlit_shader_tag_id = new ShaderTagId("SRPDefaultUnlit");

        public void Render(ScriptableRenderContext cont, Camera cam, 
            bool useDynamicBatching, bool useGPUInstancing)
        {
            context = cont;
            camera = cam;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
                return;

            Setup();
            
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
            DrawUnsupportedShaders();
            DrawGizmos();

            Submit();
        }

        #region Render Process

        private bool Cull()
        {
            if (!camera.TryGetCullingParameters(out var cull_params)) 
                return false;

            culling_results = context.Cull(ref cull_params);
            return true;
        }

        private void Setup()
        {
            context.SetupCameraProperties(camera);

            var clear_flag = camera.clearFlags;
            cmd.ClearRenderTarget(
                clearDepth: clear_flag <= CameraClearFlags.Depth, 
                clearColor: clear_flag == CameraClearFlags.SolidColor, 
                backgroundColor: clear_flag == CameraClearFlags.SolidColor ? camera.backgroundColor.linear : Color.clear);
            
            cmd.BeginSample(SampleName);
            ExecuteBuffer();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
        {
            // Draw Opaque
            var sorting_settings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawing_settings = new DrawingSettings(unlit_shader_tag_id, sorting_settings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            var filtering_settings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(culling_results, ref drawing_settings, ref filtering_settings);

            // Draw Skybox
            context.DrawSkybox(camera);

            // Draw Transparent (透明对象不写入深度，需要在天空盒之后渲染，否则会被天空盒覆盖)

            sorting_settings.criteria = SortingCriteria.CommonTransparent;
            drawing_settings.sortingSettings = sorting_settings;
            filtering_settings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(culling_results, ref drawing_settings, ref filtering_settings);
        }

        private void Submit()
        {
            cmd.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        #endregion

        #region Utils
        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        #endregion
    }
}
