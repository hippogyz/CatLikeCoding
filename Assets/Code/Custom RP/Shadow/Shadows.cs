using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.CustomShadow
{
    public class Shadows
    {
        ScriptableRenderContext context;
        CullingResults culling_results;
        ShadowSettings settings;

        #region CommandBuffer
        const string BUFFER_NAME = "Shadows";
        CommandBuffer cmd = new CommandBuffer { name = BUFFER_NAME };
        #endregion

        #region Directional

        const int MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT = 4;
        const int MAX_CASCADES = 4;

        int shadowed_directional_light_count;

        static Vector4[] cascade_culling_spheres = new Vector4[MAX_CASCADES];
        static Vector4[] cascade_data = new Vector4[MAX_CASCADES];

        static ShadowedDirectionalLight[] shadowed_directional_lights = 
            new ShadowedDirectionalLight[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];

        static Matrix4x4[] dir_shadow_matrices = new Matrix4x4[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADES];

        static readonly int shadow_distance_fade_id = Shader.PropertyToID("_ShadowDistanceFade");
        static readonly int cascade_count_id = Shader.PropertyToID("_CascadeCount");
        static readonly int cascade_culling_spheres_id = Shader.PropertyToID("_CascadeCullingSpheres");
        static readonly int cascade_data_id = Shader.PropertyToID("_CascadeData");
        static readonly int shadow_atlas_size_id = Shader.PropertyToID("_ShadowAtlasSize");
        static readonly int dir_shadow_atlas_id = Shader.PropertyToID("_DirectionalShadowAtlas");
        static readonly int dir_shadow_matrices_id = Shader.PropertyToID("_DirectionalShadowMatrices");

        static string[] directional_filter_keywords = 
        {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };

        static string[] cascade_blend_keywords =
        {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER",
        };

        static readonly string cascade_debug_keyword = "_CASCADE_DEBUG";

        #endregion

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults,
            ShadowSettings shadowSettings)
        {
            this.context = context;
            culling_results = cullingResults;
            settings = shadowSettings;

            shadowed_directional_light_count = 0;
        }

        public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            bool shadow_count_available = shadowed_directional_light_count < MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT;
            bool light_cast_shadow_available = light.shadows != LightShadows.None && light.shadowStrength > 0;
            bool culling_available = culling_results.GetShadowCasterBounds(visibleLightIndex, out var b);

            if (!(shadow_count_available && light_cast_shadow_available && culling_available))
                return Vector3.zero;

            shadowed_directional_lights[shadowed_directional_light_count] = new ShadowedDirectionalLight()
            {
                visibleLightIndex = visibleLightIndex,
                slopeScaleBias = light.shadowBias,
                nearPlaneOffset = light.shadowNearPlane,
            };

            var shadow_data = new Vector3(
                light.shadowStrength,
                settings.directional.CascadeCount * shadowed_directional_light_count,
                light.shadowNormalBias
                );

            shadowed_directional_light_count++;

            return shadow_data;
        }

        public void Render()
        {
            RenderDirectionalShadows();
        }

        public void Cleanup()
        {
            cmd.ReleaseTemporaryRT(dir_shadow_atlas_id);
            ExecuteBuffer();
        }


        private void RenderDirectionalShadows()
        {
            if(shadowed_directional_light_count == 0)
            {
                // 生成一个1X1的texture，防止没有光照时在Cleanup多余释放资源
                cmd.GetTemporaryRT(dir_shadow_atlas_id, 1, 1,
                    depthBuffer: 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
                return;
            }

            int atlas_size = (int)settings.directional.atlasSize;

            // 获取一张贴图
            cmd.GetTemporaryRT(dir_shadow_atlas_id, atlas_size, atlas_size,
                depthBuffer: 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            // 设置绘制信息
            cmd.SetRenderTarget(dir_shadow_atlas_id,
                RenderBufferLoadAction.DontCare, 
                RenderBufferStoreAction.Store);
            
            // 清空上一帧的记录
            cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.clear);
            ExecuteBuffer();


            cmd.BeginSample(BUFFER_NAME);
            ExecuteBuffer();

            // 多个光源情况下需要将深度信息保存在同一张图的不同位置
            int tile = shadowed_directional_light_count * settings.directional.CascadeCount;
            int split = tile <= 1 ? 1 : tile <= 4 ? 2 : 4;
            int tile_size = atlas_size / split;

            for(int i = 0; i < shadowed_directional_light_count; i++)
            {
                // 写入阴影深度
                RenderDirectionalShadow(i, split, tile_size);
            }

            // 传递参数给GPU
            SetShaderVariants();
            SetShaderKeywords();

            cmd.EndSample(BUFFER_NAME);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadow(int index, int split, int tileSize)
        {
            var light = shadowed_directional_lights[index];
            var shadow_drawing_settings = 
                new ShadowDrawingSettings(culling_results, light.visibleLightIndex);

            var split_count = settings.directional.CascadeCount;
            var split_ratio = settings.directional.CascadeRatios;

            float culling_factor = Mathf.Max(0, 0.8f - settings.directional.CascadeFade);

            for(int split_idx = 0; split_idx < split_count; ++split_idx)
            {
                culling_results.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    activeLightIndex: light.visibleLightIndex,
                    splitIndex: split_idx,
                    splitCount: split_count,
                    splitRatio: split_ratio,
                    shadowResolution: tileSize,
                    shadowNearPlaneOffset: light.nearPlaneOffset,
                    out Matrix4x4 view,
                    out Matrix4x4 proj,
                    out ShadowSplitData split_data);

                // 低分辨率深度图与高分辨率有重合时不再计算低分辨率的部分
                split_data.shadowCascadeBlendCullingFactor = culling_factor;

                shadow_drawing_settings.splitData = split_data;

                var tile_idx = index * split_count + split_idx;

                // 按照光源的index设置应该写入在深度贴图的哪个位置
                // 并计算贴图的相对偏移坐标
                var offset = SetTileViewport(tile_idx, split, tileSize);

                // 设置阴影贴图的变换矩阵
                dir_shadow_matrices[tile_idx] = ConvertToAtlasMatrix(proj * view, offset, split);

                // 设置Cascade Cull Sphere
                if(index == 0)
                {
                    SetCascadeData(split_idx, split_data.cullingSphere, tileSize);
                }

                // 设置光源的投影方向
                cmd.SetViewProjectionMatrices(view, proj);

                // 设置深度偏移，消除表面交叠时的错误阴影
                cmd.SetGlobalDepthBias(0, light.slopeScaleBias);
                ExecuteBuffer();

                context.DrawShadows(ref shadow_drawing_settings); // 仅作用于ShadowCaster Pass
                cmd.SetGlobalDepthBias(0, 0);
            }
        }


        private Vector2 SetTileViewport(int index, int split, float tileSize)
        {
            var y = index / split;
            var x = index - y * split;
            cmd.SetViewport(new Rect(x * tileSize, y * tileSize, tileSize, tileSize));

            // return offset
            return new Vector2(x, y);
        }

        private void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
        {
            var texel_size = 2 * cullingSphere.w / tileSize;
            var filter_size = texel_size * ((float)settings.directional.filter + 1); // 消除采样率上升后产生的自阴影

            cullingSphere.w -= filter_size;
            cullingSphere.w *= cullingSphere.w;
            cascade_culling_spheres[index] = cullingSphere;

            cascade_data[index] = new Vector4(
                1f / cullingSphere.w,
                filter_size * 1.4142136f
                );
        }

        private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            #region Matrix manipulation
            // Some API use reversed-Z for better precision as Z is stored nonlinearly.
            if (SystemInfo.usesReversedZBuffer)
            {
                // i.e. diag{1, 1, -1, 1} * m;
                m.m20 *= -1;
                m.m21 *= -1;
                m.m22 *= -1;
                m.m23 *= -1;
            }

            // Projection ranges from -1 to 1, move it to range from 0 to 1.
            // i.e.
            // 0.5  0   0  0.5
            //  0  0.5  0  0.5
            //  0   0  0.5 0.5
            //  0   0   0   1


            // Apply tile offset
            // i.e. (s = split, x = offset.x, y = offset.y)
            // 1/s  0   0  x/s
            //  0  1/s  0  y/s
            //  0   0   1   0
            //  0   0   0   1

            var scale = 1f / split;
            var x = offset.x;
            var y = offset.y;

            m.m00 = (0.5f * (m.m00 + m.m30) + x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            #endregion

            return m;
        }

        private void SetShaderVariants()
        {
            // 设置Shader中的阴影贴图变换矩阵
            cmd.SetGlobalMatrixArray(dir_shadow_matrices_id, dir_shadow_matrices);
            // 设置分级阴影的裁剪信息 (Cascade culling sphere)
            cmd.SetGlobalInt(cascade_count_id, settings.directional.CascadeCount);
            cmd.SetGlobalVectorArray(cascade_culling_spheres_id, cascade_culling_spheres);
            cmd.SetGlobalVectorArray(cascade_data_id, cascade_data);
            // 设置阴影最大距离
            cmd.SetGlobalVector(shadow_distance_fade_id,
                new Vector4(
                    1f / settings.maxDistance,
                    1f / settings.distanceFade,
                    1f / (1 - (1 - settings.directional.CascadeFade) * (1 - settings.directional.CascadeFade))
                )
                );

            // 为了Filter传递AtlasSize
            var atlas_size = (int)settings.directional.atlasSize;
            cmd.SetGlobalVector(shadow_atlas_size_id, new Vector4(atlas_size, 1f / atlas_size));
        }

        private void SetShaderKeywords()
        {
            // Filter，调整阴影柔和度（Sample数量）
            SetKeywordArray(
                (int)settings.directional.filter - 1,
                directional_filter_keywords
                );

            // Cascade Blend Mode
            SetKeywordArray(
                (int)settings.directional.CascadeBlendMode - 1,
                cascade_blend_keywords
                );

            // Cascade Debug
            SetKeywordArray(
                settings.directional.CascadeDebug ? 0 : -1,
                cascade_debug_keyword
                );
        }

        private void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        private void SetKeywordArray(int index, params string[] keywords)
        {
            for(int i = 0; i < keywords.Length; ++i)
            {
                if (index == i)
                    cmd.EnableShaderKeyword(keywords[i]);
                else
                    cmd.DisableShaderKeyword(keywords[i]);
            }
        }

        #region Inner

        struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float nearPlaneOffset;
        }

        #endregion
    }

}
