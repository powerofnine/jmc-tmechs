using TMechs.InspectorAttributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : DecoratorDrawer
{
    public override void OnGUI(Rect position)
    {
        GUI.enabled = false;
    }

    public override float GetHeight() => 0F;
}