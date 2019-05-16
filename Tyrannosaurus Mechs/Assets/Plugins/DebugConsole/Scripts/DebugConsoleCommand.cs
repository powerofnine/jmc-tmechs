using System;
using JetBrains.Annotations;

namespace DebugConsole
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class DebugConsoleCommand : Attribute
    {
        public readonly string command;
        
        public DebugConsoleCommand(string command)
        {
            this.command = command;
        }
    }
}