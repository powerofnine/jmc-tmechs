using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Builder
{
    public class TMechsBuilder : EditorWindow
    {
        private BuildTarget target;
        
        [MenuItem("Tools/TMechs/Build Wizard")]
        private static void OpenMenu()
        {
            TMechsBuilder wnd = GetWindowWithRect<TMechsBuilder>(new Rect(100F, 100F, 300F, EditorGUIUtility.singleLineHeight * 4F), true, "TMechs Build Wizard");
            wnd.Show();
        }

        private void Awake()
        {
            target = EditorUserBuildSettings.activeBuildTarget;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            PlayerSettings.bundleVersion = EditorGUILayout.TextField("Game Version", PlayerSettings.bundleVersion);
            target = (BuildTarget) EditorGUILayout.EnumPopup(target);
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            RenderToolbar();
        }
        
        private void RenderToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(position.width));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build", EditorStyles.toolbarButton, GUILayout.Width(150F)))
                DoBuild();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }
      
        private void DoBuild()
        {
            string buildPath = Path.Combine(Application.dataPath, "..", "Builds");
            
            if(Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            EditorBuildSettingsScene[] levels = EditorBuildSettings.scenes;

            BuildPipeline.BuildPlayer(levels, Path.Combine(buildPath, PlayerSettings.productName + ".exe"), target, BuildOptions.None);
        }
    }
}
