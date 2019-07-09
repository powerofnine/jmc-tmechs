// Inspector Gadgets // Copyright 2019 Kybernetik //

using InspectorGadgets.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace InspectorGadgets.Attributes
{
    /// <summary>[Pro-Only]
    /// Causes the attributed field to be drawn in a specific color.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class ColorAttribute : PropertyAttribute
    {
        /************************************************************************************************************************/

        /// <summary>The color to use.</summary>
        public readonly Color Color;

        /************************************************************************************************************************/

        /// <summary>Initialises the color with the specified red, green, and blue values.</summary>
        public ColorAttribute(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }

        /************************************************************************************************************************/

        private static Dictionary<string, Color> _NameToColor;

        /// <summary>
        /// Initialises the color using the property with the specified 'colorName'. You can specify a full name in the
        /// form "TypeName.StaticProperty" or "TypeName.StaticField", or you can just specify the name of a property in
        /// the <see cref="UnityEngine.Color"/> class such as "red".
        /// </summary>
        public ColorAttribute(string colorName)
        {
            if (_NameToColor == null)
                _NameToColor = new Dictionary<string, Color>();

            if (!_NameToColor.TryGetValue(colorName, out Color))
            {
                Color = ParseColor(colorName);
                _NameToColor.Add(colorName, Color);
            }
        }

        /************************************************************************************************************************/

        private static Color ParseColor(string colorName)
        {
            Type type;
            var dot = colorName.LastIndexOf('.');
            if (dot < 0)
            {
                type = typeof(Color);
            }
            else
            {
                var typeName = colorName.Substring(0, dot);

                type = null;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    type = assemblies[i].GetType(typeName);
                    if (type != null)
                        break;
                }

                if (type == null)
                    goto InvalidColorName;

                dot++;
                colorName = colorName.Substring(dot, colorName.Length - dot);
            }

            var property = type.GetProperty(colorName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property != null)
            {
                if (property.PropertyType == typeof(Color) || property.PropertyType == typeof(Color32))
                    return (Color)property.GetValue(null, null);
            }

            var field = type.GetField(colorName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                if (field.FieldType == typeof(Color) || field.FieldType == typeof(Color32))
                    return (Color)field.GetValue(null);
            }

            InvalidColorName:
            Debug.LogWarning("Invalid 'colorName' " + colorName);
            return Color.white;
        }

        /************************************************************************************************************************/
    }
}

#if UNITY_EDITOR
namespace InspectorGadgets.Editor.PropertyDrawers
{
    [UnityEditor.CustomPropertyDrawer(typeof(ColorAttribute))]
    internal sealed class ColorDrawer : ObjectDrawer
    {
        /************************************************************************************************************************/

        public override void OnGUI(Rect area, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var oldColor = GUI.color;
            GUI.color = (attribute as ColorAttribute).Color;
            {
                base.OnGUI(area, property, label);
            }
            GUI.color = oldColor;
        }

        /************************************************************************************************************************/

        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
        }

        /************************************************************************************************************************/
    }
}
#endif
