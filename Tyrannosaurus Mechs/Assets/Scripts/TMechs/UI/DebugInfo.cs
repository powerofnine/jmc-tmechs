using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMechs.UI
{
    [AddComponentMenu("")]
    public class DebugInfo : MonoBehaviour
    {
        private static DebugInfo instance;

        private bool isActive;
        private GUIStyle guiStyle;

        public delegate string GetDebugString();
        private readonly List<GetDebugString> debugStrings = new List<GetDebugString>();

        private static readonly string VERSION = Application.productName + " v" + Application.version;

        private void Awake()
        {
            instance = this;
            
            guiStyle = new GUIStyle {normal = {textColor = Color.white}, fontSize = 14};
            
            RegisterDebug(() => VERSION);
            RegisterDebug(() =>
            {
                string ret = "Player Info: ";

                if (Player.Player.Instance)
                    ret += "\n" + string.Join("\n", Player.Player.Instance.GetDebugInfo());
                else
                    ret += "Unavailable";

                return ret;
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                isActive = !isActive;
        }
        
        private void OnGUI()
        {
            if (!isActive)
                return;

            float yCoord = 60F;

            foreach (GetDebugString dbg in debugStrings)
            {
                string debugString = "";
                
                try
                {
                    debugString = dbg();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }

                debugString += '\n';

                Rect rect = new Rect(10F, yCoord, Screen.width,
                    guiStyle.CalcHeight(new GUIContent(debugString), Screen.width));
                
                GUI.Label(rect, debugString);
                yCoord += rect.height;
            }
        }

        public static void RegisterDebug(GetDebugString dbg)
        {
            if (!instance)
            {
                Debug.LogWarning("Attempting to register a debug string before debug info is ready...");
                return;
            }

            instance.debugStrings.Add(dbg);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize() {
            GameObject go = new GameObject("Debug Info Display");
            go.AddComponent<DebugInfo>();
            DontDestroyOnLoad(go);
        }
    }
}
