using System.Linq;
using UnityEditor;
using UnityEngine;

public class LodHelper : EditorWindow
{
    private static LOD[] savedLods;
    
    [MenuItem("Tools/TMechs/LODCopy")]
    private static void Open()
    {
        LodHelper wnd = EditorWindow.GetWindow<LodHelper>();
        
        wnd.titleContent = new GUIContent("LOD Helper");
        
        wnd.Show();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Copy LODs"))
        {
            LODGroup sel = Selection.transforms[0].GetComponent<LODGroup>();
            savedLods = sel.GetLODs();
        }

        if (GUILayout.Button("Paste LODs"))
        {
            if (savedLods == null)
            {
                Debug.LogWarning("Must first copy lods");
                return;
            }

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
        
        EditorGUILayout.EndVertical();
    }
}
