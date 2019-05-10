using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TMechs.FX
{
    public class BlurFade : MonoBehaviour
    {
        private static BlurFade instance;

        public float fadeTime = .5F;
        private Blur blurLayer;

        private float blurSize;

        private void Awake()
        {
            instance = this;
            PostProcessVolume volume = GetComponent<PostProcessVolume>();
            volume.profile.TryGetSettings(out blurLayer);

            if (blurLayer)
            {
                blurSize = blurLayer.BlurSize.value;

                blurLayer.BlurSize.value = 0F;

                volume.enabled = true;
                blurLayer.enabled.value = false;
            }
        }

        public static void Fade(bool enabled)
        {
            if (instance)
            {
                instance.StopAllCoroutines();
                instance.StartCoroutine(instance._Fade(enabled));
            }
        }

        private IEnumerator _Fade(bool enabled)
        {
            if (!blurLayer)
            {
                Debug.LogError("No blur layer detected");
                yield break;
            }

            float time = .0F;

            int multiplierStart = enabled ? 0 : 1;
            int multiplierEnd = enabled ? 1 : 0;

            blurLayer.BlurSize.value = enabled ? 0F : blurSize;
            blurLayer.enabled.value = true;

            while (time < fadeTime && blurLayer)
            {
                time += Time.unscaledDeltaTime;
                blurLayer.BlurSize.value = Mathf.Lerp(blurSize * multiplierStart, blurSize * multiplierEnd, time / fadeTime);
                yield return null;
            }

            blurLayer.enabled.value = enabled;
        }
    }
}