using TMechs.Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minProperty = property.FindPropertyRelative("x");
        SerializedProperty maxProperty = property.FindPropertyRelative("y");
        position.height = EditorGUIUtility.singleLineHeight;

        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        float min = minProperty.floatValue;
        float max = maxProperty.floatValue;

        Rect left = new Rect(position.x, position.y, position.width / 2 - 11f, position.height);
        Rect right = new Rect(position.x + position.width - left.width, position.y, left.width, position.height);
        Rect mid = new Rect(left.xMax, position.y, 22, position.height);
        min = EditorGUI.FloatField(left, min);
        EditorGUI.LabelField(mid, " to ");
        max = EditorGUI.FloatField(right, max);

        position.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.MinMaxSlider(position, GUIContent.none, ref min, ref max, min, max);

        minProperty.floatValue = min;
        maxProperty.floatValue = max;
        EditorGUI.EndProperty();
    }
}