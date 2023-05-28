using CustomRP.CustomShadow;
using UnityEditor;
using UnityEngine;

namespace ShaderEditor
{
    [CustomPropertyDrawer(typeof(ShadowSettings.Directional.CascadeInfo))]
    public class ShadowSettingsDirectionalCascadeInfoDrawer : PropertyDrawer
    {
        static float line_height = EditorGUIUtility.singleLineHeight;
        static float spacing = EditorGUIUtility.standardVerticalSpacing;

        bool fold = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!fold)
                return line_height + spacing;

            var cascadeRatioProp = property.FindPropertyRelative("ratio");
            var cascadeHeightCount = cascadeRatioProp.arraySize + 5;

            return (line_height + spacing) * cascadeHeightCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = line_height;

            fold = EditorGUI.Foldout(position, fold, "Cacade Info");
            position.y += line_height + spacing;

            if (!fold)
                return;

            var cascadeDebugProp = property.FindPropertyRelative("debug");
            var cascadeFadeProp = property.FindPropertyRelative("fade");
            var cascadeCountProp = property.FindPropertyRelative("count");
            var cascadeRatioProp = property.FindPropertyRelative("ratio");
            var cascadeBlendProp = property.FindPropertyRelative("blendMode");

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, cascadeDebugProp, true);
            position.y += line_height + spacing;

            EditorGUI.PropertyField(position, cascadeFadeProp, true);
            position.y += line_height + spacing;

            EditorGUI.PropertyField(position, cascadeCountProp, true);
            position.y += line_height + spacing;

            EditorGUI.indentLevel++;
            var cascadeCount = cascadeCountProp.intValue;
            for (int i = 0; i < cascadeRatioProp.arraySize; ++i)
            {
                EditorGUI.BeginDisabledGroup(i >= cascadeCount - 1);
                var prop = cascadeRatioProp.GetArrayElementAtIndex(i);
                prop.floatValue = EditorGUI.Slider(position, $"Ratio {i + 1}", prop.floatValue, 0, 1);
                position.y += line_height + spacing;
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;

            EditorGUI.PropertyField(position, cascadeBlendProp, true);
            position.y += line_height + spacing;

            EditorGUI.indentLevel--;
        }
    }
}
