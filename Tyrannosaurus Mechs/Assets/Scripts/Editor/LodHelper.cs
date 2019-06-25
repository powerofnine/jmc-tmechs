using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LodHelper : EditorWindow
{
    private static GUIStyle labelStyle;
    private static GUIStyle errorStyle;
    
    private int tool;
    private readonly LodTool[] tools = new LodTool[]
    {
            new CopyLodsTool(),
            new LockLodsTool()
    };

    [MenuItem("Tools/TMechs/LODCopy")]
    private static void Open()
    {
        LodHelper wnd = EditorWindow.GetWindow<LodHelper>();
        
        wnd.titleContent = new GUIContent("LOD Helper");
        
        wnd.Show();
    }

    private void OnGUI()
    {
        labelStyle = new GUIStyle(EditorStyles.label)
        {
                wordWrap = true
        };
        errorStyle = new GUIStyle()
        {
                normal = new GUIStyleState(){ textColor = Color.red }
        };
        
        GUILayout.BeginVertical();
        tool = GUILayout.Toolbar(tool, tools.Select(x => x.GetName()).ToArray());
        tools[tool].OnGui();
        GUILayout.EndVertical();
    }

    private abstract class LodTool
    {
        public abstract void OnGui();
        public abstract string GetName();
    }
    
    private class CopyLodsTool : LodTool
    {
        private static LOD[] savedLods;
        
        public override void OnGui()
        {
            EditorGUILayout.BeginVertical();

            EditorGUI.BeginDisabledGroup(Selection.transforms.Length <= 0 || !Selection.transforms[0].GetComponent<LODGroup>());
            if (GUILayout.Button("Copy"))
            {
                LODGroup sel = Selection.transforms[0].GetComponent<LODGroup>();
                savedLods = sel.GetLODs();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(savedLods == null);
            if (GUILayout.Button("Paste"))
            {
                foreach (LODGroup lods in 
                        from sel in Selection.transforms
                        let l = sel.GetComponent<LODGroup>()
                        where l
                        select l)
                {
                    LOD[] existingLods = lods.GetLODs();

                    for (int i = 0; i < existingLods.Length && i < savedLods.Length; i++)
                    {
                        existingLods[i].fadeTransitionWidth = savedLods[i].fadeTransitionWidth;
                        existingLods[i].screenRelativeTransitionHeight = savedLods[i].screenRelativeTransitionHeight;
                    }
                
                    lods.SetLODs(existingLods);
                }
            }
        
            EditorGUI.EndDisabledGroup();
            
            if (savedLods == null)
                EditorGUILayout.LabelField("Must first copy lods before pasting", errorStyle);
            
            EditorGUILayout.EndVertical();
        }

        public override string GetName() => "Copy LODs";
    }

    private class LockLodsTool : LodTool
    {
        public override void OnGui()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Adds an object to individual lod meshes that prevents them from being moved separately from the LOD group", labelStyle);
            
            
            GUILayout.Button("Lock All");
            GUILayout.Button("Unlock All");
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Button("Lock Selected");
            GUILayout.Button("Unlock Selected");
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Button("Reset All LOD transforms");
            GUILayout.Button("Reset selected LOD transforms");
            
            EditorGUILayout.EndVertical();
        }

        public override string GetName() => "Lock LODs";
    }
}
