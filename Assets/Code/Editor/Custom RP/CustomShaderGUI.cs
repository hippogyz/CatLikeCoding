using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShaderEditor
{
    public class CustomShaderGUI : ShaderGUI
    {
        MaterialEditor editor;
        Object[] materials;
        MaterialProperty[] properties;

        bool show_presets;

        #region Properties

        RenderQueue RenderQueue
        {
            set
            {
                foreach (Material material in materials)
                {
                    material.renderQueue = (int)value;
                }
            }
        }
        bool Clipping
        {
            set => SetProperty(CLIPPING, CLIPPING_KEYWORD, value);
        }
        bool PremultiplyAlpha
        {
            set => SetProperty(PREMULTIPLY_ALPHA, PREMULTIPLY_ALPHA_KEYWORD, value);
        }
        BlendMode SrcBlend
        {
            set => SetProperty(SRC_BLEND, (float)value);
        }
        BlendMode DstBlend
        {
            set => SetProperty(DST_BLEND, (float)value);
        }
        bool ZWrite
        {
            set => SetProperty(Z_WRITE, value ? 1 : 0);
        }

        #endregion

        #region Property Name

        const string CLIPPING = "_Clipping";
        const string CLIPPING_KEYWORD = "_CLIPPING";
        
        const string PREMULTIPLY_ALPHA = "_PremultiplyAlpha";
        const string PREMULTIPLY_ALPHA_KEYWORD = "_PREMULTIPLY_ALPHA";

        const string SRC_BLEND = "_SrcBlend";
        const string DST_BLEND = "_DstBlend";

        const string Z_WRITE = "_ZWrite";

        #endregion

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            editor = materialEditor;
            materials = materialEditor.targets;
            this.properties = properties;

            // Preset
            EditorGUILayout.Space();
            show_presets = EditorGUILayout.Foldout(show_presets, "Presets", true);
            if(show_presets)
            {
                OpaquePreset();
                ClipPreset();
                FadePreset();
                TransparentPreset();
            }
        }

        #region Presets

        private void OpaquePreset()
        {
            if(PresetButton("Opaque"))
            {
                RenderQueue = RenderQueue.Geometry;
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
            }
        }

        private void ClipPreset()
        {
            if(PresetButton("Clip"))
            {
                RenderQueue = RenderQueue.AlphaTest;
                Clipping = true;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
            }
        }

        private void FadePreset() // Normal Transparency
        {
            if(PresetButton("Fade"))
            {
                RenderQueue = RenderQueue.Transparent;
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.SrcAlpha;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
            }
        }

        private void TransparentPreset() // use pre-multiply alpha
        {
            if(HasProperty(PREMULTIPLY_ALPHA) && PresetButton("Transparent"))
            {
                RenderQueue = RenderQueue.Transparent;
                Clipping = false;
                PremultiplyAlpha = true;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
            }
        }

        #endregion


        #region Utils

        #region Property and Keyword
        private bool SetProperty(string name, float value)
        {
            var prop = FindProperty(name, properties, false);
            if (prop != null)
            {
                prop.floatValue = value;
                return true;
            }
            else
            {
                Debug.LogWarning($"Shader property \'{name}\' not found");
                return false;
            }
        }

        private void SetProperty(string name, string keyword, bool enable)
        {
            if (SetProperty(name, enable ? 1 : 0))
            {
                SetKeyword(keyword, enable);
            }
        }

        private void SetKeyword(string name, bool enable)
        {
            foreach(Material material in materials)
            {
                if (enable)
                    material.EnableKeyword(name);
                else
                    material.DisableKeyword(name);
            }
        }

        private bool HasProperty(string name)
        {
            return FindProperty(name, properties, false) != null;
        }
        #endregion

        #region GUI

        private bool PresetButton(string name)
        {
            if(GUILayout.Button(name))
            {
                editor.RegisterPropertyChangeUndo(name); // For ctrl-Z
                return true;
            }

            return false;
        }

        #endregion

        #endregion
    }

}
