using System.Collections;
using System.Collections.Generic;
using TMechs.InspectorAttributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NameAttribute))]
public class NameAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent((attribute as NameAttribute)?.name));
    }
}