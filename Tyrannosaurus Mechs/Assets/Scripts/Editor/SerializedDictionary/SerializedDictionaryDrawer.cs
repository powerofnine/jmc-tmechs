using UnityEditor;
using UnityEngine;

namespace SerializedDictionary
{
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private bool isFoldout;
        private float height;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            height = EditorGUIUtility.singleLineHeight;
            
            Rect rect = position;
            rect.width -= 25F;
            isFoldout = EditorGUI.Foldout(rect, isFoldout, label, true);

            if (isFoldout)
            {
                SerializedProperty keys = property.FindPropertyRelative("keys");
                SerializedProperty values = property.FindPropertyRelative("values");

                // Make sure the key/value arrays are balanced
                while(keys.arraySize > values.arraySize)
                    values.InsertArrayElementAtIndex(values.arraySize);
                while(values.arraySize > keys.arraySize)
                    keys.InsertArrayElementAtIndex(keys.arraySize);
                
                rect.x += rect.width;
                rect.width = 20F;

                if (GUI.Button(rect, "+"))
                {
                    int size = keys.arraySize;
                    
                    keys.InsertArrayElementAtIndex(size);
                    values.InsertArrayElementAtIndex(size);
                }

                rect = position;
                rect.y += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.singleLineHeight;
                

                if (keys.arraySize == 0)
                {
                    EditorGUI.LabelField(rect, "The dictionary is empty", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                EditorGUI.indentLevel++;

                bool expandedMode = values.GetArrayElementAtIndex(0).hasVisibleChildren;

                for (int i = 0; i < keys.arraySize; i++)
                {
                    Rect btnRect;
                    
                    if (!expandedMode)
                    {
                        Rect keyRect = rect;
                        keyRect.width /= 2F;
                        
                        Rect valRect = keyRect;
                        valRect.x += keyRect.width;
                        valRect.width -= 25F;
                        
                        btnRect = valRect;
                        btnRect.x += valRect.width;
                        btnRect.width = 20F;

                        EditorGUI.PropertyField(keyRect, keys.GetArrayElementAtIndex(i), new GUIContent(""));
                        EditorGUI.PropertyField(valRect, values.GetArrayElementAtIndex(i), new GUIContent(""));

                        rect.y += EditorGUIUtility.singleLineHeight;
                        height += EditorGUIUtility.singleLineHeight;
                    }
                    else
                    {
                        Rect keyRect = rect;
                        keyRect.width -= 20F;
                        
                        btnRect = keyRect;
                        btnRect.x += keyRect.width;
                        btnRect.width = 20F;
                        
                        Rect valRect = rect;
                        valRect.y += EditorGUIUtility.singleLineHeight;
                        valRect.height = EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(i), true);

                        EditorGUI.PropertyField(keyRect, keys.GetArrayElementAtIndex(i), new GUIContent("Key"));
                        EditorGUI.PropertyField(valRect, values.GetArrayElementAtIndex(i), new GUIContent(values.GetArrayElementAtIndex(i).type), true);

                        rect.y += EditorGUIUtility.singleLineHeight * 2 + valRect.height;
                        
                        if (i > 0)
                            height += EditorGUIUtility.singleLineHeight * 2 + valRect.height;
                        else
                            height += valRect.height;
                    }
                    
                    if (GUI.Button(btnRect, "-"))
                    {
                        keys.DeleteArrayElementAtIndex(i);
                        values.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }
    }
}