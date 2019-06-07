using TMechs.Environment;
using UnityEngine;

namespace TMechs.Test
{
    public class EmissSwitcher : MonoBehaviour
    {
        [ColorUsage(false, true)]
        public Color onColor = Color.green;
        [ColorUsage(false, true)]
        public Color offColor = Color.red;

        private Renderer render;
        private static readonly int EMISSIVE_COLOR = Shader.PropertyToID("_EmissiveColor");

        private void Awake()
        {
            render = GetComponent<Renderer>();

            if (render)
                render.material.SetColor(EMISSIVE_COLOR, offColor);
        }

        public void UpdateColor()
        {
            render.material.SetColor(EMISSIVE_COLOR, InteractableObject.eventToggleValue ? onColor : offColor);
        }
    }
}
