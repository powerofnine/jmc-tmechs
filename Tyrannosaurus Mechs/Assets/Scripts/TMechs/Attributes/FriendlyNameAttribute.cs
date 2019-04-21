using System;

namespace TMechs.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class FriendlyNameAttribute : Attribute
    {
        public readonly string name;

        public FriendlyNameAttribute(string name) => this.name = name;
    }
}