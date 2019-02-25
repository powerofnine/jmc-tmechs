using UnityEngine;

namespace TMechs.InspectorAttributes
{
    public class NameAttribute : PropertyAttribute
    {
        public readonly string name;
        
        public NameAttribute(string name)
        {
            this.name = name;
        }
    }
}
