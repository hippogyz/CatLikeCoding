using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    partial class CameraRenderer
    {
#if UNITY_EDITOR
        static ShaderTagId[] legacy_shader_tag_id = new ShaderTagId[]
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        static Material error_mat;
#endif

#if UNITY_EDITOR
        string SampleName { get; set; }
#else
        const string SampleName = cmd_name;
#endif

        private void DrawUnsupportedShaders()
        {
#if UNITY_EDITOR
            if (error_mat == null)
                error_mat = new Material(Shader.Find("Hidden/InternalErrorShader"));

            var drawing_settings = new DrawingSettings();
            drawing_settings.sortingSettings = new SortingSettings(camera);
            drawing_settings.overrideMaterial = error_mat;

            for (int i = 0; i < legacy_shader_tag_id.Length; ++i)
                drawing_settings.SetShaderPassName(i, legacy_shader_tag_id[i]);

            var filtering_settings = FilteringSettings.defaultValue;

            context.DrawRenderers(culling_results, ref drawing_settings, ref filtering_settings);
#endif
        }

        private void DrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
#endif
        }

        private void PrepareForSceneWindow()
        {
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                // Scene Window下绘制UI
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif
        }

        private void PrepareBuffer()
        {
#if UNITY_EDITOR
            SampleName = camera.name;
            cmd.name = SampleName; // Frame Debugger 中给不同相机分块
#endif
        }
    }
}
