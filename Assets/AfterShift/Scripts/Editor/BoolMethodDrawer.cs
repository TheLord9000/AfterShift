#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AfterShift.Runtime;

namespace AfterShift.Editor
{
    [CustomPropertyDrawer(typeof(ASBoolMethodAttribute))]
    public class ASBoolMethodDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            ASBoolMethodAttribute methodAttribute = (ASBoolMethodAttribute)attribute;

            SerializedProperty targetProperty = property.serializedObject.FindProperty(
                property.propertyPath.Replace(property.name, methodAttribute.TargetFieldName)
            );

            if (targetProperty == null || targetProperty.objectReferenceValue == null)
            {
                property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
                return;
            }

            UnityEngine.Object targetObject = targetProperty.objectReferenceValue;
            Type targetType = targetObject.GetType();

            string[] methodNames = targetType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method =>
                    method.ReturnType == typeof(bool) &&
                    method.GetParameters().Length == 0 &&
                    !method.IsSpecialName
                )
                .Select(method => method.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            if (methodNames.Length == 0)
            {
                property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
                return;
            }

            int currentIndex = Array.IndexOf(methodNames, property.stringValue);
            if (currentIndex < 0)
                currentIndex = 0;

            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, methodNames);

            property.stringValue = methodNames[selectedIndex];
        }
    }
}
#endif