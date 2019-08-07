using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Editor.T2DA
{
    [CustomEditor(typeof(T2DAImporter))]
    // ReSharper disable once InconsistentNaming
    public class T2DAEditor : ScriptedImporterEditor
    {
        private bool isFoldedOut;
        
        public override void OnInspectorGUI()
        {
            SerializedProperty textures = serializedObject.FindProperty("textures");

            textures.arraySize = EditorGUILayout.IntField("Texture Count", textures.arraySize);
            
            isFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldedOut, "Textures");

            if (isFoldedOut)
            {
                int widthCapacity = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 64F);
             
                int currentId = 0;

                for (int i = 0; i < textures.arraySize; i++)
                {
                    if (currentId == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }

                    currentId++;

                    SerializedProperty property = textures.GetArrayElementAtIndex(i);

                    property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(64F), GUILayout.Height(64F));
                    
                    if (currentId >= widthCapacity)
                    {
                        currentId = 0;

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (currentId != 0)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            string val = ((T2DAImporter) target).Validate();
            if(!string.IsNullOrWhiteSpace(val))
                EditorGUILayout.HelpBox(val, MessageType.Error);
            
            serializedObject.ApplyModifiedProperties();
            if(ApplyButton("Reimport"))
                ApplyAndImport();
        }

        public override bool HasModified() => true;
    }
}