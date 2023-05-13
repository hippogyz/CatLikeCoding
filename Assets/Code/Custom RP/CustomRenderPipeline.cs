﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        CameraRenderer renderer = new CameraRenderer();

        public CustomRenderPipeline()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                renderer.Render(context, camera);
            }
        }
    }
}
