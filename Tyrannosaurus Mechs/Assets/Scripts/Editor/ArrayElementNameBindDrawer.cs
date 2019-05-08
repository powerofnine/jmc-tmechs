using System.Globalization;
using TMechs.Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ArrayElementNameBindAttribute))]
public class ArrayElementNameBindDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ArrayElementNameBindAttribute attribute = (ArrayElementNameBindAttribute) this.attribute;

        string varPath = $"{property.propertyPath}.{attribute.variable}";
        SerializedProperty varProperty = property.serializedObject.FindProperty(varPath);

        if(varProperty != null)
            label = new GUIContent(FindName(varProperty, label.text), label.tooltip);
        else
            Debug.LogWarning($"Cannot find property: {varPath}");
        
        EditorGUI.PropertyField(position, property, label, true);
    }

    private string FindName(SerializedProperty property, string def)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();
            case SerializedPropertyType.Float:
                return property.floatValue.ToString(CultureInfo.CurrentCulture);
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return property.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue.ToString();
            case SerializedPropertyType.Enum:
                return property.enumNames[property.enumValueIndex];
            case SerializedPropertyType.Vector2:
                return property.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return property.vector3Value.ToString();
            case SerializedPropertyType.Vector4:
                return property.vector4Value.ToString();
        }

        return def;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}