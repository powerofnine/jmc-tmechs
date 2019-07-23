using System.Collections;
using System.Linq;
using UnityEngine;

namespace TMechs.FX
{
    public class SetShaderProperty : MonoBehaviour
    {
        [ColorUsage(true, true)]
        public Color color;
        public float time = 1F;
        public bool useUnscaledTime;
        public string property = "_Color";
    
        public Renderer[] renderers = {};
    
        public void Signal()
        {
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            Color[] source = renderers.Select(x => x.material.GetColor(property)).ToArray();
            float timer = 0F;

            while (timer <= time)
            {
                timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].material.SetColor(property, Color.Lerp(source[i], color, timer / time));

                yield return null;
            }
        }
    }
}
