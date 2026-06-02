#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AfterShift.Runtime;

namespace AfterShift.Editor
{
    [CustomPropertyDrawer(typeof(ASCustomInteractTitle.TitleState))]
    public class ASCustomInteractTitleStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect foldoutRect = new Rect(position.x, position.y, position.width, line);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GetFoldoutLabel(property, label), true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            float y = position.y + line + spacing;

            DrawProperty(ref y, position, property, "StateName", "State");

            SerializedProperty isFallbackState = property.FindPropertyRelative("IsFallbackState");
            DrawProperty(ref y, position, isFallbackState, "Is Fallback State");

            if (!isFallbackState.boolValue)
            {
                DrawProperty(ref y, position, property, "Target", "Target");
                DrawProperty(ref y, position, property, "BoolMethodName", "Bool Method Name");
                DrawProperty(ref y, position, property, "InvertCondition", "Invert Condition");
            }

            SerializedProperty disableInteraction = property.FindPropertyRelative("DisableInteraction");

            EditorGUI.BeginChangeCheck();
            DrawProperty(ref y, position, disableInteraction, "Disable Interaction");
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            if (!disableInteraction.boolValue)
            {
                y += spacing;

                Rect titleHeaderRect = GetRect(position, y, line);
                EditorGUI.LabelField(titleHeaderRect, "Titles", EditorStyles.boldLabel);
                y += line + spacing;

                DrawOptionalGString(ref y, position, property, "OverrideTitle", "Title", "Override Title", "Title");
                DrawOptionalGString(ref y, position, property, "OverrideUseTitle", "UseTitle", "Override Use Title", "Use Title");
                DrawOptionalGString(ref y, position, property, "OverrideExamineTitle", "ExamineTitle", "Override Examine Title", "Examine Title");
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return line;

            float height = line + spacing; // foldout

            SerializedProperty isFallbackState = property.FindPropertyRelative("IsFallbackState");
            SerializedProperty disableInteraction = property.FindPropertyRelative("DisableInteraction");

            height += Line(); // StateName
            height += Line(); // IsFallbackState

            if (!isFallbackState.boolValue)
            {
                height += Line(); // Target
                height += Line(); // BoolMethodName
                height += Line(); // InvertCondition
            }

            height += Line(); // DisableInteraction

            if (!disableInteraction.boolValue)
            {
                height += spacing;
                height += Line(); // Titles header

                height += OptionalGStringHeight(property, "OverrideTitle", "Title");
                height += OptionalGStringHeight(property, "OverrideUseTitle", "UseTitle");
                height += OptionalGStringHeight(property, "OverrideExamineTitle", "ExamineTitle");
            }

            return height;

            float Line()
            {
                return line + spacing;
            }
        }

        private static GUIContent GetFoldoutLabel(SerializedProperty property, GUIContent defaultLabel)
        {
            SerializedProperty stateName = property.FindPropertyRelative("StateName");

            if (stateName != null && !string.IsNullOrWhiteSpace(stateName.stringValue))
                return new GUIContent(stateName.stringValue);

            return defaultLabel;
        }

        private static void DrawProperty(ref float y, Rect position, SerializedProperty parent, string propertyName, string label)
        {
            SerializedProperty prop = parent.FindPropertyRelative(propertyName);
            DrawProperty(ref y, position, prop, label);
        }

        private static void DrawProperty(ref float y, Rect position, SerializedProperty property, string label)
        {
            float height = EditorGUI.GetPropertyHeight(property, true);
            Rect rect = GetRect(position, y, height);

            EditorGUI.PropertyField(rect, property, new GUIContent(label), true);

            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static void DrawOptionalGString(
            ref float y,
            Rect position,
            SerializedProperty parent,
            string toggleName,
            string valueName,
            string toggleLabel,
            string valueLabel)
        {
            SerializedProperty toggle = parent.FindPropertyRelative(toggleName);
            SerializedProperty value = parent.FindPropertyRelative(valueName);

            DrawProperty(ref y, position, toggle, toggleLabel);

            if (!toggle.boolValue)
                return;

            DrawProperty(ref y, position, value, valueLabel);
        }

        private static float OptionalGStringHeight(SerializedProperty parent, string toggleName, string valueName)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty toggle = parent.FindPropertyRelative(toggleName);
            SerializedProperty value = parent.FindPropertyRelative(valueName);

            float height = EditorGUI.GetPropertyHeight(toggle, true) + spacing;

            if (toggle.boolValue)
                height += EditorGUI.GetPropertyHeight(value, true) + spacing;

            return height;
        }

        private static Rect GetRect(Rect position, float y, float height)
        {
            return new Rect(position.x, y, position.width, height);
        }
    }
}
#endif