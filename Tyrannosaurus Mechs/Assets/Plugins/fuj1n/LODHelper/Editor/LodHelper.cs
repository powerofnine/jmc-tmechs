using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.fuj1n.LODHelper.Editor
{
    public class LodHelper : EditorWindow
    {
        private static GUIStyle labelStyle;
        private static GUIStyle errorStyle;
        private static GUIStyle warningStyle;

        private int tool;
        private readonly LodTool[] tools =
        {
                new CopyLodsTool(),
                new LockLodsTool(),
                new CreateLodsTool()
        };

        [MenuItem("Tools/LOD Helper")]
        private static void Open()
        {
            LodHelper wnd = GetWindow<LodHelper>();

            wnd.titleContent = new GUIContent("LOD Helper");

            wnd.Show();
        }

        private void OnGUI()
        {
            labelStyle = new GUIStyle(EditorStyles.label)
            {
                    wordWrap = true
            };
            errorStyle = new GUIStyle(labelStyle)
            {
                    normal = new GUIStyleState() {textColor = Color.red}
            };
            warningStyle = new GUIStyle(labelStyle)
            {
                    normal = new GUIStyleState() {textColor = Color.yellow}
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
                EditorGUILayout.LabelField("Copies LOD levels from one LOD group and pastes them onto all selected LOD groups whilst keeping the renderers of the destination groups untouched", labelStyle);

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
                    if (savedLods == null)
                        return;

                    LODGroup[] groups = (from sel in Selection.transforms
                            let l = sel.GetComponent<LODGroup>()
                            where l
                            select l).ToArray();

                    // ReSharper disable once CoVariantArrayConversion
                    Undo.RecordObjects(groups, "Pasted LODs");

                    foreach (LODGroup lods in groups)
                    {
                        LOD[] existingLods = lods.GetLODs();

                        for (int i = 0; i < existingLods.Length && i < savedLods.Length; i++)
                        {
                            existingLods[i].fadeTransitionWidth = savedLods[i].fadeTransitionWidth;
                            existingLods[i].screenRelativeTransitionHeight = savedLods[i].screenRelativeTransitionHeight;
                        }

                        lods.SetLODs(existingLods);
                        EditorSceneManager.MarkSceneDirty(lods.gameObject.scene);
                    }
                }

                EditorGUI.EndDisabledGroup();

                if (savedLods == null)
                    EditorGUILayout.LabelField("Must first copy LODs before being able to paste", errorStyle);

                EditorGUILayout.LabelField("Pasting LODs onto LOD groups with different amount of LOD levels will only paste the parts that exist in both source and destination.", warningStyle);

                EditorGUILayout.EndVertical();
            }

            public override string GetName() => "Copy LODs";
        }

        private class LockLodsTool : LodTool
        {
            public override void OnGui()
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Adds an object to individual LOD meshes that prevents them from being moved separately from the LOD group", labelStyle);
            
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("It is preferable to lock LODs by checking the checkbox as that does not modify the scene, use it if you don't want to be able to unlock specific LOD groups", labelStyle);

                LodHelperSettings.Instance.activelyPreventLodSelection = EditorGUILayout.ToggleLeft("Actively prevent LOD selection", LodHelperSettings.Instance.activelyPreventLodSelection);

                if (GUILayout.Button("Lock All"))
                {
                    Lock(true);
                }

                if (GUILayout.Button("Unlock All"))
                {
                    Unlock(true);
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Lock Selected"))
                {
                    Lock(false);
                }

                if (GUILayout.Button("Unlock Selected"))
                {
                    Unlock(false);
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Reset All LOD transforms"))
                {
                    Reset(true);
                }

                if (GUILayout.Button("Reset selected LOD transforms"))
                {
                    Reset(false);
                }

                EditorGUILayout.EndVertical();
            }

            private void Lock(bool all)
            {
                LODGroup[] lods = GetGroups(all);
                Undo.RecordObjects(lods.SelectMany(l => l.GetLODs(), (l, ld) => new {l, ld}).SelectMany(t => t.ld.renderers, (t, rn) => new {t, rn}).Where(t => !t.rn.GetComponent<LockLodSelection>()).Select(t => (Object)t.rn.gameObject).ToArray(), "Locked LODs");
            
                foreach (LODGroup grp in lods)
                {
                    foreach (LOD lod in grp.GetLODs())
                    {
                        foreach (Renderer ren in lod.renderers)
                        {
                            if (ren.GetComponent<LockLodSelection>())
                                continue;

                            ren.gameObject.AddComponent<LockLodSelection>();
                            EditorSceneManager.MarkSceneDirty(ren.gameObject.scene);
                        }
                    }
                }
            }

            private void Unlock(bool all)
            {
                LODGroup[] lods = GetGroups(all);
                Undo.RecordObjects(lods.SelectMany(l => l.GetLODs(), (l, ld) => new {l, ld}).SelectMany(t => t.ld.renderers, (t, rn) => new {t, rn}).Where(t => t.rn.GetComponent<LockLodSelection>()).Select(t => (Object)t.rn.gameObject).ToArray(), "Unlocked LODs");
            
                foreach (LODGroup grp in lods)
                {
                    foreach (LOD lod in grp.GetLODs())
                    {
                        foreach (Renderer ren in lod.renderers)
                        {
                            LockLodSelection trans = ren.GetComponent<LockLodSelection>();

                            if (trans)
                            {
                                DestroyImmediate(trans);
                                EditorSceneManager.MarkSceneDirty(ren.gameObject.scene);
                            }
                        }
                    }
                }
            }

            private void Reset(bool all)
            {
                LODGroup[] lods = GetGroups(all);
                Undo.RecordObjects(lods.SelectMany(l => l.GetLODs(), (l, ld) => new {l, ld}).SelectMany(t => t.ld.renderers, (t, rn) => new {t, rn}).Select(t => (Object)t.rn.gameObject).ToArray(), "Reset LOD transforms");
            
                foreach (LODGroup grp in lods)
                {
                    foreach (LOD lod in grp.GetLODs())
                    {
                        foreach (Renderer ren in lod.renderers)
                        {
                            ren.gameObject.transform.localPosition = Vector3.zero;
                            ren.gameObject.transform.localEulerAngles = Vector3.zero;
                            ren.gameObject.transform.localScale = Vector3.one;

                            EditorSceneManager.MarkSceneDirty(ren.gameObject.scene);
                        }
                    }
                }
            }

            private LODGroup[] GetGroups(bool all)
            {
                if (all)
                    return FindObjectsOfType<LODGroup>();

                return (from sel in Selection.transforms
                        let l = sel.GetComponent<LODGroup>()
                        where l
                        select l).ToArray();
            }

            public override string GetName() => "Lock LODs";
        }

        private class CreateLodsTool : LodTool
        {
            public override void OnGui()
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Creates LOD groups on every selected object with a LOD stage for every renderer underneath said object", labelStyle);

                if (GUILayout.Button("Create LODs"))
                {
                    foreach (GameObject sel in Selection.gameObjects.Where(x => !x.GetComponent<Renderer>() && !x.GetComponent<LODGroup>()))
                    {
                        Renderer[] rens = sel.GetComponentsInChildren<Renderer>();
                        if (rens == null || rens.Length == 0)
                            continue;

                        LODGroup group = sel.AddComponent<LODGroup>();
                        LOD[] lods = new LOD[rens.Length];

                        for (int i = 0; i < rens.Length; i++)
                        {
                            lods[i].renderers = new[] {rens[i]};
                            lods[i].screenRelativeTransitionHeight = 1 - (float) i / rens.Length;
                        }

                        group.SetLODs(lods);
                    }
                }
            }

            public override string GetName() => "Create LODs";
        }
    
        [InitializeOnLoadMethod]
        private static void Init()
        {
            Selection.selectionChanged += OnUpdateSelection;
        }

        private static void OnUpdateSelection()
        {
            // Need to hook into next update as selectionChanged does not allow changing selection
            if(LodHelperSettings.Instance.activelyPreventLodSelection)
                EditorApplication.delayCall += FilterSelection;
        }

        private static void FilterSelection()
        {
            Selection.objects = Selection.objects.Select(s =>
            {
                GameObject go = s as GameObject;

                if (!go)
                    return s;

                LODGroup grp = go.GetComponentInParent<LODGroup>();
            
                if (!grp)
                    return s;

                return grp.gameObject;
            }).ToArray();
        }
    }
}