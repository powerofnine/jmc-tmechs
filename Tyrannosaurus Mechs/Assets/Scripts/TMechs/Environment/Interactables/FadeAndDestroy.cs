using System.Collections;
using System.Linq;
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
            Renderer[] ren = GetComponentsInChildren<Renderer>().Where(x => x.material).ToArray();

            if (ren.Length == 0)
                yield break;

            Color[] startColor = ren.Select(x => x.material.GetColor(MAIN_COLOR)).ToArray();
            Color[] endColor = startColor.Select(x =>
            {
                x.a = 0F;
                return x;
            }).ToArray();
            
            float time = 0F;
            while (time <= fadeTime)
            {
                time += Time.deltaTime;

                for (int i = 0; i < ren.Length; i++)
                {
                    ren[i].material.SetColor(MAIN_COLOR, Color.Lerp(startColor[i], endColor[i], time / fadeTime));
                }

                yield return null;
            }
            
            Destroy(gameObject);
        }
        
    }
}
