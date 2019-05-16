using UnityEngine;

namespace DebugConsole
{
    public static class BuiltinCommands
    {
        [DebugConsoleCommand("log")]
        public static void Log(string message)
        {
            Log(message, Color.white);
        }

        [DebugConsoleCommand("log")]
        public static void Log(string message, Color c)
        {
            DebugConsole.Instance.AddMessage(message, c);
        }

        [DebugConsoleCommand("clear")]
        public static void Clear()
        {
            DebugConsole.Instance.Clear();
        }

        [DebugConsoleCommand("fullscreen")]
        public static void SetFullscreen(FullScreenMode mode)
        {
            Screen.fullScreenMode = mode;
        }

        [DebugConsoleCommand("quit")]
        public static void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        [DebugConsoleCommand("killTaggedObjects")]
        public static void KillObjectByTag(string tag)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
                Object.Destroy(go);
        }

        [DebugConsoleCommand("killObject")]
        public static void KillObject(string name)
        {
            Object.Destroy(GameObject.Find(name));
        }
    }
}