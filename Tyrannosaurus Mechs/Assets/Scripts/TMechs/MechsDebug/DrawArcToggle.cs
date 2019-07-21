using fuj1n.MinimalDebugConsole;
using UnityEngine;

namespace TMechs.MechsDebug
{
    public class DrawArcToggle : MonoBehaviour
    {
        private static bool drawArc;
        private Renderer render;

        private void Awake()
        {
            render = GetComponent<Renderer>();

            if (!render)
            {
                Destroy(this);
                return;
            }

            render.enabled = drawArc;
        }

        [DebugConsoleCommand("drawARC")]
        private static void DrawArc()
        {
            drawArc = !drawArc;

            foreach (DrawArcToggle toggle in FindObjectsOfType<DrawArcToggle>())
                toggle.render.enabled = drawArc;

            DebugConsole.Instance.AddMessage($"ARC drawing {(drawArc ? "<#00FF00>enabled</color>" : "<#FF0000>disabled</color>")}", Color.white);
        }
    }
}