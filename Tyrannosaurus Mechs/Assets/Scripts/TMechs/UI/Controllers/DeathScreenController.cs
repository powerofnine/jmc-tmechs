using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace TMechs.UI.Controllers
{
    public class DeathScreenController : MonoBehaviour
    {
        public CanvasGroup canvas;
        public Volume volume;
        
        private void Awake()
        {
            canvas.alpha = 0F;
            StartCoroutine(FadeVolume());
        }

        private IEnumerator FadeVolume()
        {
            float time = 0F;
            while (time < 2F)
            {
                time += Time.unscaledDeltaTime;

                if (volume)
                    volume.weight = time / 2F;
                
                yield return null;
            }
        }
        
        public IEnumerator FadeCanvas()
        {
            float time = 0F;
            while (time < 1F)
            {
                time += Time.unscaledDeltaTime;

                if (canvas)
                    canvas.alpha = time / 1F;
                
                yield return null;
            }
        }
    }
}
