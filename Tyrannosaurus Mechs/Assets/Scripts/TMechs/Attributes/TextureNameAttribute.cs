using System;

namespace TMechs.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class TextureNameAttribute : Attribute
    {
        public readonly string name;

        public TextureNameAttribute(string name) => this.name = name;
    }
}
