using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Editor.Builder
{
    public class TMechsDeployer : EditorWindow
    {
        private string version;
        private string tag;
        
        private string message;
        private string description;
        
        private int buildId;
        private string[] builds;

        private Vector2 messageScroll;
        
        [MenuItem("Tools/TMechs/Deploy Wizard")]
        private static void OpenMenu()
        {
            TMechsDeployer wnd = GetWindowWithRect<TMechsDeployer>(new Rect(100F, 100F, 600F, EditorGUIUtility.singleLineHeight * 30F), true, "TMechs Deploy Wizard");
            wnd.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            if (string.IsNullOrWhiteSpace(version))
                version = PlayerSettings.bundleVersion;
            if (string.IsNullOrWhiteSpace(tag))
                tag = $"v{version}";

            version = EditorGUILayout.TextField("Version", version);
            tag = EditorGUILayout.TextField("Tag", tag);

            if (builds == null)
            {
                string buildPath = Path.Combine(Application.dataPath, "..", "Builds");
                builds = Directory.GetFiles(buildPath, "*.zip");
            }

            buildId = EditorGUILayout.IntPopup("Build File", buildId, builds.Select(Path.GetFileNameWithoutExtension).ToArray(), Enumerable.Range(0, builds.Length).ToArray());

            EditorGUILayout.LabelField("Specific Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Changelog");
            EditorGUILayout.BeginScrollView(messageScroll);
            message = EditorGUILayout.TextArea(message, GUILayout.Height(EditorGUIUtility.singleLineHeight * 20F));
            EditorGUILayout.EndScrollView();

            description = EditorGUILayout.TextField("Optional Comment", description);
            
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            RenderToolbar();
        }
        
        private void RenderToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(position.width));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Deploy", EditorStyles.toolbarButton, GUILayout.Width(150F)))
                DoDeploy();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DoDeploy()
        {
            //TODO deploy to GitHub

            HttpWebRequest hook = WebRequest.CreateHttp(DeploySecrets.BuildHookUrl);
            hook.ContentType = "application/json";
            hook.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(hook.GetRequestStream()))
            {
                ServerData data = new ServerData()
                {
                    id = DeploySecrets.BuildHookSecret,
                    description = description,
                    title = $"Playtest - {tag}",
                    message = message,
                    url = $"https://github.com/powerofnine/jmc-tmechs/releases/tag/{tag}"
                };
                
                string json = JsonConvert.SerializeObject(data);
                
                streamWriter.Write(json.Replace("\r\n", "\n"));
            }

            hook.GetResponse();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private struct ServerData
        {
            public string id;
            public string description;
            public string title;
            public string message;
            public string url;
        }
    }
}
