using UnityEngine;

namespace TMechs.InspectorAttributes
{
    public class IdAttribute : PropertyAttribute
    {
        public readonly string displayName;
        
        public IdAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }
}