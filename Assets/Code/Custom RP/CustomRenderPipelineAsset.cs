using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        public bool useSPRBatching;
        public bool useDynamicBatching;
        public bool useGPUInstancing;


        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(useSPRBatching, useDynamicBatching, useGPUInstancing);
        }
    }

}
