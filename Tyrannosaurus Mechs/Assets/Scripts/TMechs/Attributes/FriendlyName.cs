using System;

namespace TMechs.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class FriendlyName : Attribute
    {
        public readonly string name;

        public FriendlyName(string name) => this.name = name;
    }
}