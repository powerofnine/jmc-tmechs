namespace fuj1n.MinimalDebugConsole
{
    using System;
    using JetBrains.Annotations;

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    [PublicAPI]
    public class DebugConsoleCommand : Attribute
    {
        public readonly string command;

        public DebugConsoleCommand(string command)
        {
            this.command = command;
        }
    }
}