using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.Builder
{
    public static class DeploySecrets
    {
        private static bool isLoaded;

        public static string BuildHookUrl
        {
            get => buildHookUrl;
            set
            {
                if (buildHookUrl == value)
                    return;

                buildHookUrl = value;
                EditorPrefs.SetString("TMechs.Deploy.BuildHookUrl", buildHookUrl);
            }
        }

        public static string BuildHookSecret
        {
            get => buildHookSecret;
            set
            {
                if (buildHookSecret == value)
                    return;

                buildHookSecret = value;
                EditorPrefs.SetString("TMechs.Deploy.BuildHookSecret", buildHookSecret);
            }
        }
        
        private static string buildHookUrl;
        private static string buildHookSecret;

        private static void Load()
        {
            buildHookUrl = EditorPrefs.GetString("TMechs.Deploy.BuildHookUrl");
            buildHookSecret = EditorPrefs.GetString("TMechs.Deploy.BuildHookSecret");

            isLoaded = true;
        }
        
        [SettingsProvider]
        private static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Preferences/TMechs/Deploy", SettingsScope.User)
            {
                    guiHandler = ctx =>
                    {
                        if (!isLoaded)
                            Load();

                        BuildHookUrl = EditorGUILayout.TextField("Build Hook URL", buildHookUrl);
                        BuildHookSecret = EditorGUILayout.TextField("Build Hook Secret", buildHookSecret);
                    }
            };
        }

        static DeploySecrets()
        {
            Load();
        }
    }
}
