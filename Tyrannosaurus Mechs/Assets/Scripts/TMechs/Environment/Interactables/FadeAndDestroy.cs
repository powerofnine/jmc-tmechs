using System.Collections;
using UnityEngine;

namespace TMechs.Environment.Interactables
{
    public class FadeAndDestroy : MonoBehaviour
    {
        public float fadeTime = 1F;
        public bool activateOnAwake;
        private bool isFading;
        
        private static readonly int MAIN_COLOR = Shader.PropertyToID("_MainColor");

        private void Awake()
        {
            if (activateOnAwake)
                Fade();
        }

        public void Fade()
        {
            if (isFading)
                return;

            isFading = true;
            StartCoroutine(Fade_Do());
        }

        private IEnumerator Fade_Do()
        {
            Renderer ren = GetComponent<Renderer>();

            if (!ren || !ren.material)
                yield break;

            Color startColor = ren.material.GetColor(MAIN_COLOR);
            Color endColor = startColor;
            endColor.a = 0F;


            float time = 0F;
            while (time <= fadeTime)
            {
                time += Time.deltaTime;
                ren.material.SetColor(MAIN_COLOR, Color.Lerp(startColor, endColor, time / fadeTime));

                yield return null;
            }
            
            Destroy(gameObject);
        }
        
    }
}
