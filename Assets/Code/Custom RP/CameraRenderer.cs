using CustomRP.CustomLight;
using CustomRP.CustomShadow;
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
        Lighting lighting = new Lighting();

        const string cmd_name = "Render Camera";
        CommandBuffer cmd = new CommandBuffer { name = cmd_name };

        CullingResults culling_results;

        static ShaderTagId unlit_shader_tag_id = new ShaderTagId("SRPDefaultUnlit");
        static ShaderTagId lit_shader_tag_id = new ShaderTagId("CustomLit");


        public void Render(ScriptableRenderContext cont, Camera cam, 
            bool useDynamicBatching, bool useGPUInstancing,
            ShadowSettings shadowSettings)
        {
            context = cont;
            camera = cam;

            #region Editor
            PrepareBuffer(); // EditorOnly: 预处理
            PrepareForSceneWindow(); // EditorOnly: SceneWindow设置
            #endregion

            if (!Cull(shadowSettings.maxDistance)) // 裁剪
                return;

            #region Editor
            Sample(true); // Editor Profile: 开始Sample
            #endregion

            lighting.Setup(context, culling_results, shadowSettings); // 设置光照和阴影 (阴影包含对context的操作)

            Setup(); // 初始化相机 (注意放在阴影预处理之后，否则会被阴影中的写入操作影响)

            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing); // 绘制物体

            #region Editor
            DrawUnsupportedShaders(); // EditorOnly: 绘制不支持的shaders
            DrawGizmos(); // EditorOnly: 绘制Gizmos
            #endregion

            lighting.Cleanup();

            #region Editor
            Sample(false); // Editor Profile:  结束Sample
            #endregion

            Submit(); // 执行上述设置
        }

        #region Camera Render Process

        private bool Cull(float maxShadowDistance)
        {
            if (!camera.TryGetCullingParameters(out var cull_params)) 
                return false;

            cull_params.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
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
            
            // cmd.BeginSample(SampleName);
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
            drawing_settings.SetShaderPassName(1, lit_shader_tag_id);
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
            //cmd.EndSample(SampleName);
            //ExecuteBuffer();
            context.Submit();
        }

        #endregion

        #region Utils

        private void Sample(bool enable)
        {
            if (enable)
                cmd.BeginSample(SampleName);
            else
                cmd.EndSample(SampleName);

            ExecuteBuffer();
        }

        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        #endregion
    }
}
