using CustomRP.CustomShadow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        [Space]
        [SerializeField] bool useSPRBatching;
        [SerializeField] bool useDynamicBatching;
        [SerializeField] bool useGPUInstancing;

        [Space]
        [SerializeField] ShadowSettings shadowSettings = default;

        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(
                useSPRBatching, useDynamicBatching, useGPUInstancing, 
                shadowSettings);
        }
    }

}
