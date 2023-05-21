using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HippoGYZ.EditorHelper
{
    public static class ShaderEditor
    {
        const string EDITOR_ENV_VAR_NAME = "VSCode_Path";

        [OnOpenAsset(1)]
        public static bool OpenShaderWithVSCode(int instanceId, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceId);

            if (string.IsNullOrWhiteSpace(path))
                return false;

            var extension = Path.GetExtension(path);
            if (!(extension.Equals(".shader") || extension.Equals(".hlsl")))
                return false;

            var editor_path = Environment.GetEnvironmentVariable(EDITOR_ENV_VAR_NAME);

            if(string.IsNullOrWhiteSpace(editor_path))
            {
                Debug.LogError($"[Env error] \"{EDITOR_ENV_VAR_NAME}\" not found. ");
                return false;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = Path.Combine(editor_path, "Code.exe");
            startInfo.Arguments = "\"" + Path.Combine(Directory.GetParent(Application.dataPath).ToString(), path) + "\"";
            process.StartInfo = startInfo;
            process.Start();

            return true;
        }

    }
}

