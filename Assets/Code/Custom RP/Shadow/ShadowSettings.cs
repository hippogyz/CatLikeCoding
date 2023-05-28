using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomRP.CustomShadow
{
    [Serializable]
    public class ShadowSettings
    {
        [Min(0f)]
        public float maxDistance = 100f;

        [Range(0.001f, 1f)]
        public float distanceFade = 0.1f;

        public Directional directional = new Directional()
        {
            atlasSize = TextureSize._1024,
            filter = FilterMode.PCF2x2,
            cascadeInfo = new Directional.CascadeInfo
            {
                fade = 0.1f,
                count = 1,
                ratio = new List<float> { 0.1f, 0.25f, 0.5f },
                blendMode = CascadeBlendMode.Hard,
            }
        };

        #region Inner

        public enum TextureSize
        {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192,
        }

        public enum FilterMode
        {
            // percentage closer filtering
            PCF2x2, PCF3x3, PCF5x5, PCF7x7
        }

        public enum CascadeBlendMode
        {
            Hard,
            Soft,
            Dither
        }

        [Serializable]
        public struct Directional
        {
            public TextureSize atlasSize;
            public FilterMode filter;

            // Cascade
            public bool CascadeDebug => cascadeInfo.debug;
            public float CascadeFade => cascadeInfo.fade;
            public int CascadeCount => cascadeInfo.count;
            public Vector3 CascadeRatios => cascadeInfo.Ratios;
            public CascadeBlendMode CascadeBlendMode => cascadeInfo.blendMode;

            #region CascadeInfo

            public CascadeInfo cascadeInfo;

            [Serializable]
            public struct CascadeInfo
            {
                public bool debug;
                [Range(0.001f, 1f)] public float fade;
                [Range(1, 4)] public int count;
                public List<float> ratio;
                public CascadeBlendMode blendMode;

                public Vector3 Ratios => new Vector3(ratio[0], ratio[1], ratio[2]);
            }
            #endregion
        }

        #endregion
    }

}
