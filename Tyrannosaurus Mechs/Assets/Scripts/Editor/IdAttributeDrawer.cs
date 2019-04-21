using System;
using TMechs.InspectorAttributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IdAttribute))]
public class IdAttributeDrawer : PropertyDrawer
{
    private const float BUTTON_WIDTH = 25F;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.width -= BUTTON_WIDTH;
        Rect buttonPos = position;
        buttonPos.x += position.width;
        buttonPos.width = BUTTON_WIDTH;
        
        if (GUI.Button(buttonPos, "↻"))
            property.stringValue = Guid.NewGuid().ToString();
        
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, new GUIContent((attribute as IdAttribute)?.displayName));
    }
}