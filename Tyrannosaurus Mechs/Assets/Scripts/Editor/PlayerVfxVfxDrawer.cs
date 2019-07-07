using TMechs.Player;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerVfx.Vfx))]
public class PlayerVfxVfxDrawer : PropertyDrawer
{
    private bool foldout;
    private float height;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        height = EditorGUIUtility.singleLineHeight;
        position.height = EditorGUIUtility.singleLineHeight;
        
        foldout = EditorGUI.Foldout(position, foldout, label, true);

        if (foldout)
        {
            SerializedProperty anchor = property.FindPropertyRelative("anchor");
            SerializedProperty isDynamic = property.FindPropertyRelative("isDynamic");

            position.y += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.singleLineHeight;
            
            position.y += position.height;
            height += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, isDynamic);

            if (!isDynamic.boolValue)
            {
                SerializedProperty staticEffect = property.FindPropertyRelative("effect");
                position.y += EditorGUIUtility.singleLineHeight;
                height += position.height;
                EditorGUI.PropertyField(position, staticEffect);
            }
            else
            {
                SerializedProperty dynamicEffect = property.FindPropertyRelative("dynamicEffect");
                position.y += position.height;
                position.height = EditorGUI.GetPropertyHeight(dynamicEffect, true);
                height += position.height;
                EditorGUI.PropertyField(position, dynamicEffect, true);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}