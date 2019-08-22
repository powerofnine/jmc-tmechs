using System.Collections;
using UnityEngine;

namespace TMechs.UI.FX
{
    public class UiFade : MonoBehaviour
    {
        public float fadeTime = 0.5F;
        public float delay = 0F;

        private CanvasRenderer[] renderers;

        private void Awake()
        {
            renderers = GetComponentsInChildren<CanvasRenderer>();

            StartCoroutine(Fade(0F, 1F));
        }

        private IEnumerator Fade(float startAlpha, float endAlpha, bool kill = false)
        {
            float time = 0F;

            while (time < delay)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            time = 0F;
            
            while (time < fadeTime)
            {
                time += Time.unscaledDeltaTime;

                foreach (CanvasRenderer renderer in renderers)
                    renderer.SetAlpha(Mathf.Lerp(startAlpha, endAlpha, time / fadeTime));

                yield return null;
            }

            if (kill)
                Destroy(gameObject);
        }

        public void Kill()
        {
            StartCoroutine(Fade(1F, 0F, true));
        }
    }
}