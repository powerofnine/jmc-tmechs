using System;
using System.Collections.Generic;
using TMechs.Types;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Radius))]
public class RadiusPropertyDrawer : PropertyDrawer
{
    private static bool fresh;
    private static List<Tuple<Transform, float, Color, bool>> gizmos = new List<Tuple<Transform,float,Color, bool>>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!fresh)
        {
            gizmos.Clear();
            fresh = true;
        }

        SerializedProperty radius = property.FindPropertyRelative("radius");
        SerializedProperty visualize = property.FindPropertyRelative("visualize");
        SerializedProperty visualizeColor = property.FindPropertyRelative("visualizeColor");

        Rect rect = position;
        rect.width -= 100F;

        EditorGUI.PropertyField(rect, property.FindPropertyRelative("radius"), label);

        rect.x += rect.width + 5F;
        rect.width = 35F;

        visualize.boolValue = GUI.Toggle(rect, visualize.boolValue, "Vis");

        rect.x += rect.width - 10F;
        rect.width = 60F;
        if (visualize.boolValue)
        {
            EditorGUI.PropertyField(rect, visualizeColor, new GUIContent(""));

            Color color = visualizeColor.colorValue;
            color.a = 1F;
            if (Selection.activeTransform)
                gizmos.Add(new Tuple<Transform, float, Color, bool>(Selection.activeTransform, radius.floatValue, color, property.FindPropertyRelative("renderAsLine").boolValue));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    [DrawGizmo(GizmoType.Selected)]
    private static void OnDrawGizmos(Transform component, GizmoType gizmoType)
    {
        fresh = false;
        
        foreach((Transform transform, float radius, Color color, bool line) in gizmos)
        {
            if(transform != Selection.activeTransform)
                continue;
            
            Gizmos.color = color;
            
            if(line)
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * radius);
            else
                Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}