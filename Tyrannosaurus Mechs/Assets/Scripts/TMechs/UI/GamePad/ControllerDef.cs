using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Rewired.Data.Mapping;
using TMechs.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace TMechs.UI.GamePad
{
    public class ControllerDef : ScriptableObject
    {
        public string padName;
        public string guid;
        public ButtonLayout layout;

        [Serializable]
        public enum ButtonLayout
        {
            [FriendlyName("PlayStation 3")]
            [TextureName("buttons.ps3")]
            Ps3,
            [FriendlyName("PlayStation 4")]
            [TextureName("buttons.ps4")]
            Ps4,
            [FriendlyName("Xbox 360")]
            [TextureName("buttons.x360")]
            Xbox360,
            [FriendlyName("Xbox One")]
            [TextureName("buttons.xone")]
            XboxOne,
            [FriendlyName("Nintendo")]
            [TextureName("buttons.nintendo")]
            Nintendo,
            [TextureName("buttons.missingno")]
            Unsupported
        }

#if UNITY_EDITOR
        [MenuItem("Tools/TMechs/Process Rewired Controller Files")]
        private static void Reprocess()
        {
            string[] assets = AssetDatabase.FindAssets("t:HardwareJoystickMap").Select(AssetDatabase.GUIDToAssetPath).ToArray();

            foreach (string path in assets)
            {
                HardwareJoystickMap map = AssetDatabase.LoadAssetAtPath<HardwareJoystickMap>(path);

                if (!map)
                    continue;

                string file = Path.Combine("Assets/Resources/Controllers", Path.GetFileName(path) ?? "MISSINGNO");

                ButtonLayout layout = ButtonLayout.Unsupported;

                if (!string.IsNullOrWhiteSpace(AssetDatabase.AssetPathToGUID(file)))
                {
                    ControllerDef def = AssetDatabase.LoadAssetAtPath<ControllerDef>(path);

                    if (def)
                    {
                        layout = def.layout;
                        DestroyImmediate(def);
                    }

                    AssetDatabase.DeleteAsset(file);
                }

                ControllerDef definition = CreateInstance<ControllerDef>();

                definition.padName = map.ControllerName;
                definition.guid = map.Guid.ToString();
                definition.layout = layout;
                
                AssetDatabase.CreateAsset(definition, file);
            }
        }
#endif
    }
}