using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.Builder
{
    public class TMechsBuilder : EditorWindow
    {
        private const string zipArgs = "a {file}.zip * -tzip -mx=9 -bsp1 -bse2 -bso0";
        private static Regex zipStatus = new Regex(@"(?<p>[0-9]+)%", RegexOptions.Compiled);
        
        private Process process;
        private BuildTarget target;

        private float zipProgressPercent;
        private string zipProgressInfo;
        private bool zipProgressHide;
        private bool zipProgressVisible;
        
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
            BuildTarget last = EditorUserBuildSettings.activeBuildTarget;
            string buildPath = Path.Combine(Application.dataPath, "..", "Builds");
            
            if(Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            EditorBuildSettingsScene[] levels = EditorBuildSettings.scenes;

            BuildReport report = BuildPipeline.BuildPlayer(levels, Path.Combine(buildPath, PlayerSettings.productName + ".exe"), target, BuildOptions.None);

            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(last), last);
            if (report.summary.result != BuildResult.Succeeded)
                return;

            string zipperPath = Path.Combine(Application.dataPath, "..", "Tools", "7za.exe");

            if (process != null && !process.HasExited)
            {
                Debug.LogWarning("A process is still running, skipping zipping...");
                return;
            }
            if (!File.Exists(zipperPath))
            {
                Debug.LogWarning("Cannot find 7za, skipping zipping...");
                return;
            }

            process = new Process
            {
                    StartInfo =
                    {
                            FileName = zipperPath,
                            Arguments = zipArgs.Replace("{file}", PlayerSettings.productName.Replace(" ", "_") + "_" + PlayerSettings.bundleVersion),
                            WorkingDirectory = buildPath,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
            };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;
            process.Exited += ExitHandler;

            EditorUtility.DisplayProgressBar("TMechs Build - Zipping", "", 0F);
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        private static void OutputHandler(object o, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            
            string data = e.Data.Trim();

            if (string.IsNullOrWhiteSpace(data))
                return;

            Match m = zipStatus.Match(e.Data ?? "");
            if (m.Success)
            {
                int percent = int.Parse(m.Groups["p"].Value);

                EditorApplication.delayCall += () => EditorUtility.DisplayProgressBar("TMechs Build - Zipping", data, percent / 100F);
            }
        }

        private static void ErrorHandler(object o, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                Debug.LogError(e.Data);
        }

        private static void ExitHandler(object o, EventArgs e)
        {
            EditorApplication.delayCall += EditorUtility.ClearProgressBar;
        }

        [MenuItem("Tools/TMechs/Clear Progress Bar")]
        private static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
