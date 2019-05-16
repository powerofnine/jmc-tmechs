namespace fuj1n.MinimalDebugConsole
{
    using UnityEngine;

    public static class BuiltinCommands
    {
        /// <summary>
        /// Logs a <paramref name="message"/> to the debug console and colors it white
        /// </summary>
        [DebugConsoleCommand("log")]
        public static void Log(string message)
        {
            Log(message, Color.white);
        }

        /// <summary>
        /// Logs a <paramref name="message"/> to the debug console and colors it <paramref name="c"/>
        /// </summary>
        [DebugConsoleCommand("log")]
        public static void Log(string message, Color c)
        {
            DebugConsole.Instance.AddMessage(message, c);
        }

        /// <summary>
        /// Clears the debug console
        /// </summary>
        [DebugConsoleCommand("clear")]
        public static void Clear()
        {
            DebugConsole.Instance.Clear();
        }

        /// <summary>
        /// Changes the fullscreen <paramref name="mode"/> of the game
        /// </summary>
        [DebugConsoleCommand("fullscreen")]
        public static void SetFullscreen(FullScreenMode mode)
        {
            Screen.fullScreenMode = mode;
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        [DebugConsoleCommand("quit")]
        public static void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Finds all objects with a given <paramref name="tag"/> and kills them
        /// </summary>
        [DebugConsoleCommand("killTaggedObjects")]
        public static void KillObjectByTag(string tag)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
                Object.Destroy(go);
        }

        /// <summary>
        /// Finds the first object with a given <paramref name="name"/> and kills it
        /// </summary>
        [DebugConsoleCommand("killObject")]
        public static void KillObject(string name)
        {
            Object.Destroy(GameObject.Find(name));
        }
    }
}