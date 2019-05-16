using UnityEngine;

namespace DebugConsole
{
    public class BuiltinCommands
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
            
        }
    }
}