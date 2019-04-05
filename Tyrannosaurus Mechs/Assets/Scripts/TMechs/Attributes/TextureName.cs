using System;

namespace TMechs.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class TextureName : Attribute
    {
        public readonly string name;

        public TextureName(string name) => this.name = name;
    }
}
