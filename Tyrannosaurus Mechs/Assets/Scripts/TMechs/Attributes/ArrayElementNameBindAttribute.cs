using UnityEngine;

namespace TMechs.Attributes
{
    public class ArrayElementNameBindAttribute : PropertyAttribute
    {
        public readonly string variable;

        public ArrayElementNameBindAttribute(string variable)
            => this.variable = variable;
    }
}