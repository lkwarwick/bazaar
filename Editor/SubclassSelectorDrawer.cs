using System;
using System.Linq;
using Bazaar;
using UnityEditor;
using UnityEngine;

namespace Bazaar.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    internal sealed class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "SubclassSelector requires a string field.");
                return;
            }

            var selector = (SubclassSelectorAttribute)attribute;
            Type[] types = TypeCache.GetTypesDerivedFrom(selector.BaseType)
                .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
                .OrderBy(type => type.FullName)
                .ToArray();

            string[] typeNames = types.Select(type => type.AssemblyQualifiedName).ToArray();
            string[] options = new string[types.Length + 1];
            options[0] = "None";
            for (int index = 0; index < types.Length; index++)
                options[index + 1] = types[index].Name;

            int typeIndex = string.IsNullOrEmpty(property.stringValue)
                ? -1
                : Array.IndexOf(typeNames, property.stringValue);
            int selectedIndex = string.IsNullOrEmpty(property.stringValue)
                ? 0
                : typeIndex >= 0 ? typeIndex + 1 : -1;

            if (selectedIndex < 0)
            {
                Array.Resize(ref options, options.Length + 1);
                options[^1] = "Missing Type";
                selectedIndex = options.Length - 1;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = newIndex == 0 ? string.Empty : typeNames[newIndex - 1];
            EditorGUI.EndProperty();
        }
    }
}
